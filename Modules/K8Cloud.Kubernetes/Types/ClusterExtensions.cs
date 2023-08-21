using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Services;
using k8s;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace K8Cloud.Kubernetes.Types;

[ExtendObjectType(typeof(ClusterRecord))]
internal class ClusterExtensions
{
    private static TimeSpan CacheExpiration = TimeSpan.FromSeconds(30);

    public async Task<ClusterStatus?> GetStatus(
        [Parent] ClusterRecord clusterRecord,
        [Service] ILogger<ClusterExtensions> logger,
        [Service] IMapper mapper,
        [Service] IDistributedCache distributedCache,
        [Service] KubernetesClientsService kubernetesClientsService,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var key = $"clusterStatus:{clusterRecord.Id}";
            var cacheValue = await distributedCache
                .GetStringAsync(key, cancellationToken)
                .ConfigureAwait(false);
            if (!string.IsNullOrEmpty(cacheValue))
            {
                var deserializedData = JsonSerializer.Deserialize<ClusterStatus>(cacheValue);
                if (deserializedData != null)
                {
                    return deserializedData;
                }
            }

            var client = kubernetesClientsService.GetClient(clusterRecord.Id);
            var response = await client.CoreV1
                .ListNodeAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            var nodes = mapper.Map<NodeInfo[]>(response.Items);

            var status = new ClusterStatus
            {
                IsOperative = !Array.Exists(
                    nodes,
                    node =>
                        Array.Exists(
                            node.Conditions,
                            condition => condition.Type == "Ready" && !condition.IsOperative
                        )
                ),
                Nodes = nodes
            };

            await distributedCache
                .SetStringAsync(
                    key,
                    JsonSerializer.Serialize(status),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = CacheExpiration
                    },
                    cancellationToken
                )
                .ConfigureAwait(false);

            return status;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get cluster status");
            return null;
        }
    }
}

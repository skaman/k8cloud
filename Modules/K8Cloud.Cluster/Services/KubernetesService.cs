using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace K8Cloud.Cluster.Services;

internal class KubernetesService : IKubernetesService
{
    // TODO: move the cache on the query side? graphql have something?
    // TODO: get the cache time from configuration?
    private static TimeSpan ClusterStatusCacheExpiration = TimeSpan.FromSeconds(30);

    private readonly IKubernetesClientsService _kubernetesClientsService;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;

    public KubernetesService(
        IKubernetesClientsService kubernetesClientsService,
        IMapper mapper,
        IDistributedCache cache
    )
    {
        _kubernetesClientsService = kubernetesClientsService;
        _mapper = mapper;
        _cache = cache;
    }

    /// <summary>
    /// Get the status of the cluster.
    /// The status is requested from the cluster and cached for 30 seconds.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cluster status.</returns>
    public async Task<ClusterStatus> GetStatusAsync(
        Guid clusterId,
        CancellationToken cancellationToken = default
    )
    {
        // try to get the status from the cache
        var key = $"clusterStatus:{clusterId}";
        var cacheValue = await _cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(cacheValue))
        {
            var deserializedData = JsonSerializer.Deserialize<ClusterStatus>(cacheValue);
            if (deserializedData != null)
            {
                return deserializedData;
            }
        }

        // get the status from the cluster
        var client = _kubernetesClientsService.GetClient(clusterId);
        var response = await client.CoreV1
            .ListNodeAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var nodes = _mapper.Map<NodeInfo[]>(response.Items);

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

        // update the cache
        await _cache
            .SetStringAsync(
                key,
                JsonSerializer.Serialize(status),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ClusterStatusCacheExpiration
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        return status;
    }

    public async Task<NamespaceResource> GetNamespaceAsync(Guid clusterId, string name)
    {
        try
        {
            var client = _kubernetesClientsService.GetClient(clusterId);
            var @namespace = await client.CoreV1.ReadNamespaceAsync(name).ConfigureAwait(false);
            return _mapper.Map<NamespaceResource>(@namespace);
        }
        catch (Exception e)
        {
            throw Exceptions.KubernetesException.FromException(e);
        }
    }

    public async Task CreateNamespaceAsync(NamespaceResource resource)
    {
        var client = _kubernetesClientsService.GetClient(resource.ClusterId);
        var @namespace = _mapper.Map<V1Namespace>(resource);

        try
        {
            await client.CoreV1.CreateNamespaceAsync(@namespace).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw Exceptions.KubernetesException.FromException(e);
        }
    }

    public async Task UpdateNamespaceAsync(NamespaceResource resource)
    {
        var client = _kubernetesClientsService.GetClient(resource.ClusterId);
        var @namespace = _mapper.Map<V1Namespace>(resource);

        try
        {
            await client.CoreV1
                .PatchNamespaceAsync(
                    new V1Patch(@namespace, V1Patch.PatchType.ApplyPatch),
                    resource.Name,
                    fieldManager: "k8cloud-namespace-field-manager"
                //force: true
                )
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            throw Exceptions.KubernetesException.FromException(e);
        }
    }
}

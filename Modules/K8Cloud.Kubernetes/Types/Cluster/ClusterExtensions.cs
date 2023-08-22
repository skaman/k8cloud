using HotChocolate;
using HotChocolate.Types;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Services;
using Microsoft.Extensions.Logging;

namespace K8Cloud.Kubernetes.Types.Cluster;

[ExtendObjectType(typeof(ClusterResource))]
internal class ClusterExtensions
{
    public async Task<ClusterStatus?> GetStatus(
        [Parent] ClusterResource clusterRecord,
        [Service] ILogger<ClusterExtensions> logger,
        [Service] ClusterService clusterService,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await clusterService
                .GetStatusAsync(clusterRecord.Id, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get cluster status");
            return null;
        }
    }
}

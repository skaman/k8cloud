using HotChocolate;
using HotChocolate.Types;
using K8Cloud.Cluster.Services;
using K8Cloud.Contracts.Kubernetes.Data;
using Microsoft.Extensions.Logging;

namespace K8Cloud.Cluster.Types.Cluster;

[ExtendObjectType(typeof(ClusterResource))]
internal class ClusterExtensions
{
    /// <summary>
    /// Cluster status.
    /// </summary>
    /// <param name="clusterRecord">Cluster record.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="kubernetesService">Kubernetes service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<ClusterStatus?> GetStatus(
        [Parent] ClusterResource clusterRecord,
        [Service] ILogger<ClusterExtensions> logger,
        [Service] IKubernetesService kubernetesService,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await kubernetesService
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

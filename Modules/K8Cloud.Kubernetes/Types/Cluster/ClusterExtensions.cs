using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotChocolate;
using HotChocolate.Types;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Extensions;
using K8Cloud.Kubernetes.Services;
using K8Cloud.Shared.Database;
using k8s.KubeConfigModels;
using Microsoft.Extensions.Logging;

namespace K8Cloud.Kubernetes.Types.Cluster;

[ExtendObjectType(typeof(ClusterResource))]
internal class ClusterExtensions
{
    /// <summary>
    /// Cluster status.
    /// </summary>
    /// <param name="clusterRecord">Cluster record.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="clusterService">Cluster service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<ClusterResourceStatus?> GetStatus(
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

    /// <summary>
    /// Namespaces.
    /// </summary>
    /// <param name="clusterRecord">Cluster record.</param>
    /// <param name="dbContext">Database context.</param>
    /// <param name="mapper">Mapper.</param>
    public IQueryable<NamespaceResource> GetNamespaces(
        [Parent] ClusterResource clusterRecord,
        K8CloudDbContext dbContext,
        [Service] IMapper mapper
    )
    {
        return dbContext
            .NamespacesReadOnly()
            .Where(x => x.ClusterId == clusterRecord.Id)
            .ProjectTo<NamespaceResource>(mapper.ConfigurationProvider);
    }
}

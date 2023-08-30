using HotChocolate.Data;
using HotChocolate.Types;
using K8Cloud.Shared.Database;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using AutoMapper.QueryableExtensions;
using HotChocolate;
using K8Cloud.Shared.GraphQL.Exceptions;
using K8Cloud.Cluster.Extensions;

namespace K8Cloud.Cluster.Types.Cluster;

[QueryType]
internal static class ClusterQuery
{
    /// <summary>
    /// Get the clusters.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <param name="mapper">Mapper.</param>
    /// <returns>Clusters query.</returns>
    [UseOffsetPaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<ClusterResource> GetClusters(
        K8CloudDbContext dbContext,
        [Service] IMapper mapper
    )
    {
        return dbContext
            .ClustersReadOnly()
            .ProjectTo<ClusterResource>(mapper.ConfigurationProvider);
    }

    /// <summary>
    /// Get a cluster by id.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="dbContext">Database context.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cluster resource.</returns>
    public static async Task<ClusterResource> GetClusterById(
        Guid clusterId,
        K8CloudDbContext dbContext,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var result = await dbContext
            .ClustersReadOnly()
            .SingleOrDefaultAsync(x => x.Id == clusterId, cancellationToken)
            .ConfigureAwait(false);
        if (result == null)
        {
            throw new ResourceNotFoundException(clusterId);
        }
        return mapper.Map<ClusterResource>(result);
    }
}

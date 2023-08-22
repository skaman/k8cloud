using HotChocolate.Data;
using HotChocolate.Types;
using K8Cloud.Shared.Database;
using K8Cloud.Kubernetes.Database;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using AutoMapper.QueryableExtensions;
using HotChocolate;
using K8Cloud.Shared.GraphQL.Exceptions;

namespace K8Cloud.Kubernetes.Types.Cluster;

[QueryType]
internal static class ClusterQuery
{
    [UseOffsetPaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<ClusterResource> GetClusters(
        K8CloudDbContext dbContext,
        [Service] IMapper mapper
    )
    {
        return dbContext.ClustersReadOnly().ProjectTo<ClusterResource>(mapper.ConfigurationProvider);
    }

    public static async Task<ClusterResource> GetClusterById(
        Guid id,
        K8CloudDbContext dbContext,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var result = await dbContext
            .ClustersReadOnly()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        if (result == null)
        {
            throw new ResourceNotFoundException(id);
        }
        return mapper.Map<ClusterResource>(result);
    }
}

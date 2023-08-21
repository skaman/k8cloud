using HotChocolate.Data;
using HotChocolate.Types;
using K8Cloud.Shared.Database;
using K8Cloud.Kubernetes.Database;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using AutoMapper.QueryableExtensions;
using HotChocolate;

namespace K8Cloud.Kubernetes.Types;

[QueryType]
internal static class ClusterQuery
{
    [UseOffsetPaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<ClusterRecord> GetClusters(
        K8CloudDbContext dbContext,
        [Service] IMapper mapper
    )
    {
        return dbContext.ClustersReadOnly().ProjectTo<ClusterRecord>(mapper.ConfigurationProvider);
    }

    public static async Task<ClusterRecord> GetClusterById(
        Guid id,
        K8CloudDbContext dbContext,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var result = await dbContext
            .ClustersReadOnly()
            .SingleAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
        return mapper.Map<ClusterRecord>(result);
    }
}

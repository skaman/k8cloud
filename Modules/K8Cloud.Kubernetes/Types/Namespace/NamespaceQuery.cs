using AutoMapper;
using HotChocolate.Data;
using HotChocolate.Types;
using HotChocolate;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Shared.Database;
using K8Cloud.Kubernetes.Extensions;
using AutoMapper.QueryableExtensions;
using K8Cloud.Shared.GraphQL.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace K8Cloud.Kubernetes.Types.Namespace;

[QueryType]
internal static class NamespaceQuery
{
    /// <summary>
    /// Get the namespaces.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="dbContext">Database context.</param>
    /// <param name="mapper">Mapper.</param>
    /// <returns>Namespaces query.</returns>
    [UseOffsetPaging]
    [UseFiltering]
    [UseSorting]
    public static IQueryable<NamespaceResource> GetNamespaces(
        Guid clusterId,
        K8CloudDbContext dbContext,
        [Service] IMapper mapper
    )
    {
        return dbContext
            .NamespacesReadOnly()
            .Where(x => x.ClusterId == clusterId)
            .ProjectTo<NamespaceResource>(mapper.ConfigurationProvider);
    }

    /// <summary>
    /// Get a namespace by id.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="namespaceId">Namespace ID.</param>
    /// <param name="dbContext">Database context.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Namespace resource.</returns>
    public static async Task<NamespaceResource> GetNamespaceById(
        Guid clusterId,
        Guid namespaceId,
        K8CloudDbContext dbContext,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var result = await dbContext
            .NamespacesReadOnly()
            .SingleOrDefaultAsync(
                x => x.Id == namespaceId && x.ClusterId == clusterId,
                cancellationToken
            )
            .ConfigureAwait(false);
        if (result == null)
        {
            throw new ResourceNotFoundException(namespaceId);
        }
        return mapper.Map<NamespaceResource>(result);
    }
}

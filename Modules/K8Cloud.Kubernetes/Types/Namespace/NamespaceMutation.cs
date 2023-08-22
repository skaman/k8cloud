using AutoMapper;
using FluentValidation;
using HotChocolate;
using HotChocolate.Types;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Services;
using K8Cloud.Shared.Database;

namespace K8Cloud.Kubernetes.Types.Namespace;

[MutationType]
internal static class NamespaceMutation
{
    /// <summary>
    /// Create a namespace.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="data">Namespace data.</param>
    /// <param name="dbContext">Database context.</param>
    /// <param name="namespaceService">Namespace service.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created namespace resource.</returns>
    [Error(typeof(ValidationException))]
    public static async Task<NamespaceResource> CreateNamespaceAsync(
        Guid clusterId,
        NamespaceData data,
        K8CloudDbContext dbContext,
        [Service] NamespaceService namespaceService,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var @namespace = await namespaceService
            .CreateAsync(clusterId, data, cancellationToken)
            .ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return mapper.Map<NamespaceResource>(@namespace);
    }

    /// <summary>
    /// Validate the namespace data for creation.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="data">Namespace data.</param>
    /// <param name="namespaceService">Namespace service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation data.</returns>
    public static Task<FluentValidation.Results.ValidationResult> ValidateCreateNamespaceAsync(
        Guid clusterId,
        NamespaceData data,
        [Service] NamespaceService namespaceService,
        CancellationToken cancellationToken
    )
    {
        return namespaceService.ValidateCreateAsync(clusterId, data, cancellationToken);
    }

    /// <summary>
    /// Update a namespace.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="namespaceId">Namespace ID.</param>
    /// <param name="data">Namespace data.</param>
    /// <param name="version">Namespace resource version.</param>
    /// <param name="dbContext">Database context.</param>
    /// <param name="namespaceService">Namespace service.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated namespace resource.</returns>
    [Error(typeof(ValidationException))]
    public static async Task<NamespaceResource> UpdateNamespaceAsync(
        Guid clusterId,
        Guid namespaceId,
        NamespaceData data,
        string version,
        K8CloudDbContext dbContext,
        [Service] NamespaceService namespaceService,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var @namespace = await namespaceService
            .UpdateAsync(clusterId, namespaceId, data, version, cancellationToken)
            .ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return mapper.Map<NamespaceResource>(@namespace);
    }

    /// <summary>
    /// Validate the namespace data for update.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="namespaceId">Namespace ID.</param>
    /// <param name="data">Namespace data.</param>
    /// <param name="namespaceService">Namespace service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation data.</returns>
    public static Task<FluentValidation.Results.ValidationResult> ValidateUpdateNamespaceAsync(
        Guid clusterId,
        Guid namespaceId,
        NamespaceData data,
        [Service] NamespaceService namespaceService,
        CancellationToken cancellationToken
    )
    {
        return namespaceService.ValidateUpdateAsync(
            clusterId,
            namespaceId,
            data,
            cancellationToken
        );
    }

    /// <summary>
    /// Delete a namespace.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="namespaceId">Namespace ID.</param>
    /// <param name="dbContext">Database context.</param>
    /// <param name="namespaceService">Namespace service.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted namespace resource.</returns>
    public static async Task<NamespaceResource> DeleteNamespaceAsync(
        Guid clusterId,
        Guid namespaceId,
        K8CloudDbContext dbContext,
        [Service] NamespaceService namespaceService,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var @namespace = await namespaceService
            .DeleteAsync(clusterId, namespaceId, cancellationToken)
            .ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return mapper.Map<NamespaceResource>(@namespace);
    }
}

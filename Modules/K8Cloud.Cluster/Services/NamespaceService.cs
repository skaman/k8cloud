using AutoMapper;
using K8Cloud.Cluster.Entities;
using K8Cloud.Cluster.Extensions;
using K8Cloud.Cluster.Validators;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Shared.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace K8Cloud.Cluster.Services;

/// <summary>
/// Manages the namespaces.
/// </summary>
internal class NamespaceService : INamespaceService
{
    private readonly K8CloudDbContext _dbContext;
    private readonly INamespaceDataValidator _namespaceDataValidator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;

    public NamespaceService(
        K8CloudDbContext dbContext,
        INamespaceDataValidator namespaceDataValidator,
        IPublishEndpoint publishEndpoint,
        IMapper mapper
    )
    {
        _dbContext = dbContext;
        _namespaceDataValidator = namespaceDataValidator;
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new namespace.
    /// </summary>
    /// <param name="clusterId">Cluster ID where to create the new namespace.</param>
    /// <param name="data">Namespace data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created namespace entity.</returns>
    public async Task<NamespaceEntity> CreateAsync(
        Guid clusterId,
        NamespaceData data,
        CancellationToken cancellationToken = default
    )
    {
        // validate the data
        await _namespaceDataValidator
            .ValidateForCreateAsync(
                clusterId,
                data,
                options: o => o.ThrowOnFailures(),
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        // create and add the namespace
        var @namespace = _mapper.Map<NamespaceEntity>(data);
        @namespace.Id = NewId.NextGuid();
        @namespace.ClusterId = clusterId;
        await _dbContext.AddAsync(@namespace, cancellationToken).ConfigureAwait(false);

        // save for retrieve version and dates
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // publish the event
        await _publishEndpoint
            .Publish(
                new NamespaceCreated
                {
                    Resource = _mapper.Map<NamespaceResource>(@namespace),
                    Timestamp = DateTime.UtcNow
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // save for events in the outbox
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return @namespace;
    }

    /// <summary>
    /// Validates the namespace data for creation.
    /// </summary>
    /// <param name="clusterId">Cluster ID where to create the new namespace.</param>
    /// <param name="data">Namespace data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public Task<FluentValidation.Results.ValidationResult> ValidateCreateAsync(
        Guid clusterId,
        NamespaceData data,
        CancellationToken cancellationToken = default
    )
    {
        return _namespaceDataValidator.ValidateForCreateAsync(
            clusterId,
            data,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Updates data of the namespace.
    /// </summary>
    /// <param name="clusterId">Cluster ID where the namespace is located.</param>
    /// <param name="namespaceId">Namespace ID.</param>
    /// <param name="data">Namespace data.</param>
    /// <param name="version">Resource version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated namespace entity.</returns>
    public async Task<NamespaceEntity> UpdateAsync(
        Guid clusterId,
        Guid namespaceId,
        NamespaceData data,
        string version,
        CancellationToken cancellationToken = default
    )
    {
        // validate the data
        await _namespaceDataValidator
            .ValidateForUpdateAsync(
                clusterId,
                namespaceId,
                data,
                options: o => o.ThrowOnFailures(),
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        // update the namespace
        var @namespace = await _dbContext
            .Namespaces()
            .SingleAsync(x => x.Id == namespaceId && x.ClusterId == clusterId, cancellationToken)
            .ConfigureAwait(false);
        _mapper.Map(data, @namespace);

        _dbContext.SetEntityVersion(@namespace, uint.Parse(version));
        _dbContext.Update(@namespace);

        // save for retrieve version and dates
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // publish the event
        await _publishEndpoint
            .Publish(
                new NamespaceUpdated
                {
                    Resource = _mapper.Map<NamespaceResource>(@namespace),
                    Timestamp = DateTime.UtcNow
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // save for events in the outbox
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return @namespace;
    }

    /// <summary>
    /// Validates the namespace data for update.
    /// </summary>
    /// <param name="clusterId">Cluster ID where the namespace is located.</param>
    /// <param name="namespaceId">Namespace ID.</param>
    /// <param name="data">Namespace data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public Task<FluentValidation.Results.ValidationResult> ValidateUpdateAsync(
        Guid clusterId,
        Guid namespaceId,
        NamespaceData data,
        CancellationToken cancellationToken = default
    )
    {
        return _namespaceDataValidator.ValidateForUpdateAsync(
            clusterId,
            namespaceId,
            data,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Deletes the namespace.
    /// </summary>
    /// <param name="clusterId">Cluster ID where the namespace is located.</param>
    /// <param name="namespaceId">Namespace ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted namespace entity.</returns>
    public async Task<NamespaceEntity> DeleteAsync(
        Guid clusterId,
        Guid namespaceId,
        CancellationToken cancellationToken = default
    )
    {
        // delete the namespace
        var @namespace = await _dbContext
            .Namespaces()
            .SingleAsync(x => x.Id == namespaceId && x.ClusterId == clusterId, cancellationToken)
            .ConfigureAwait(false);
        _dbContext.Remove(@namespace);

        // publish the event
        await _publishEndpoint
            .Publish(
                new NamespaceDeleted
                {
                    Resource = _mapper.Map<NamespaceResource>(@namespace),
                    Timestamp = DateTime.UtcNow
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // save the changes
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return @namespace;
    }
}

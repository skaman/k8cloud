using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Kubernetes.Entities;
using K8Cloud.Kubernetes.Extensions;
using K8Cloud.Kubernetes.Validators;
using K8Cloud.Shared.Database;
using k8s.KubeConfigModels;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace K8Cloud.Kubernetes.Services;

/// <summary>
/// Manages the clusters.
/// </summary>
internal class ClusterService : IClusterService
{
    private readonly K8CloudDbContext _dbContext;
    private readonly IClusterDataValidator _clusterDataValidator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;

    public ClusterService(
        K8CloudDbContext dbContext,
        IClusterDataValidator clusterDataValidator,
        IPublishEndpoint publishEndpoint,
        IMapper mapper
    )
    {
        _dbContext = dbContext;
        _clusterDataValidator = clusterDataValidator;
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new cluster.
    /// </summary>
    /// <param name="data">Cluster data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created cluster entity.</returns>
    public async Task<ClusterEntity> CreateAsync(
        ClusterData data,
        CancellationToken cancellationToken = default
    )
    {
        // validate the data
        await _clusterDataValidator
            .ValidateForCreateAsync(
                data,
                options: o => o.ThrowOnFailures(),
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        // create and add the cluster
        var cluster = _mapper.Map<ClusterEntity>(data);
        cluster.Id = NewId.NextGuid();
        await _dbContext.AddAsync(cluster, cancellationToken).ConfigureAwait(false);

        // save for retrieve version and dates
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // publish the event
        await _publishEndpoint
            .Publish(
                new ClusterCreated
                {
                    Resource = _mapper.Map<ClusterResource>(cluster),
                    Timestamp = DateTime.UtcNow
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // save for events in the outbox
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return cluster;
    }

    /// <summary>
    /// Validates the cluster data for creation.
    /// </summary>
    /// <param name="data">Cluster data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public Task<FluentValidation.Results.ValidationResult> ValidateCreateAsync(
        ClusterData data,
        CancellationToken cancellationToken = default
    )
    {
        return _clusterDataValidator.ValidateForCreateAsync(
            data,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Updates data of the cluster.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="data">Data to update.</param>
    /// <param name="version">Resource version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated cluster entity.</returns>
    public async Task<ClusterEntity> UpdateAsync(
        Guid clusterId,
        ClusterData data,
        string version,
        CancellationToken cancellationToken = default
    )
    {
        // validate the data
        await _clusterDataValidator
            .ValidateForUpdateAsync(
                clusterId,
                data,
                options: o => o.ThrowOnFailures(),
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        // update the cluster
        var cluster = await _dbContext
            .Clusters()
            .SingleAsync(x => x.Id == clusterId, cancellationToken)
            .ConfigureAwait(false);
        _mapper.Map(data, cluster);

        _dbContext.SetEntityVersion(cluster, uint.Parse(version));
        _dbContext.Update(cluster);

        // save for retrieve version and dates
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // publish the event
        await _publishEndpoint
            .Publish(
                new ClusterUpdated
                {
                    Resource = _mapper.Map<ClusterResource>(cluster),
                    Timestamp = DateTime.UtcNow
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // save for events in the outbox
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return cluster;
    }

    /// <summary>
    /// Validates the cluster data for update.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="data">Data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public Task<FluentValidation.Results.ValidationResult> ValidateUpdateAsync(
        Guid clusterId,
        ClusterData data,
        CancellationToken cancellationToken = default
    )
    {
        return _clusterDataValidator.ValidateForUpdateAsync(
            clusterId,
            data,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Deletes the cluster.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted cluster entity.</returns>
    public async Task<ClusterEntity> DeleteAsync(
        Guid clusterId,
        CancellationToken cancellationToken = default
    )
    {
        // delete the cluster
        var cluster = await _dbContext
            .Clusters()
            .SingleAsync(x => x.Id == clusterId, cancellationToken)
            .ConfigureAwait(false);
        _dbContext.Remove(cluster);

        // publish the event
        await _publishEndpoint
            .Publish(
                new ClusterDeleted
                {
                    Resource = _mapper.Map<ClusterResource>(cluster),
                    Timestamp = DateTime.UtcNow
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // save the changes
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return cluster;
    }
}

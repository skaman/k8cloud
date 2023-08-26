using AutoMapper;
using FluentValidation;
using HotChocolate;
using HotChocolate.Types;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Services;
using K8Cloud.Shared.Database;

namespace K8Cloud.Kubernetes.Types.Cluster;

[MutationType]
internal static class ClusterMutation
{
    /// <summary>
    /// Create a cluster.
    /// </summary>
    /// <param name="data">Cluster data.</param>
    /// <param name="dbContext">Database context.</param>
    /// <param name="clusterService">Cluster service.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created cluster resource.</returns>
    [Error(typeof(ValidationException))]
    public static async Task<ClusterResource> CreateClusterAsync(
        ClusterData data,
        K8CloudDbContext dbContext,
        [Service] IClusterService clusterService,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var cluster = await clusterService
            .CreateAsync(data, cancellationToken)
            .ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return mapper.Map<ClusterResource>(cluster);
    }

    /// <summary>
    /// Validate the cluster data for creation.
    /// </summary>
    /// <param name="data">Cluster data.</param>
    /// <param name="clusterService">Cluster service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation data.</returns>
    public static Task<FluentValidation.Results.ValidationResult> ValidateCreateClusterAsync(
        ClusterData data,
        [Service] IClusterService clusterService,
        CancellationToken cancellationToken
    )
    {
        return clusterService.ValidateCreateAsync(data, cancellationToken);
    }

    /// <summary>
    /// Update a cluster.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="data">Cluster data.</param>
    /// <param name="version">Cluster resource version.</param>
    /// <param name="dbContext">Database context.</param>
    /// <param name="clusterService">Cluster service.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated cluster resource.</returns>
    [Error(typeof(ValidationException))]
    public static async Task<ClusterResource> UpdateClusterAsync(
        Guid clusterId,
        ClusterData data,
        string version,
        K8CloudDbContext dbContext,
        [Service] IClusterService clusterService,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var cluster = await clusterService
            .UpdateAsync(clusterId, data, version, cancellationToken)
            .ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return mapper.Map<ClusterResource>(cluster);
    }

    /// <summary>
    /// Validate the cluster data for update.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="data">Cluster data.</param>
    /// <param name="clusterService">Cluster service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation data.</returns>
    public static Task<FluentValidation.Results.ValidationResult> ValidateUpdateClusterAsync(
        Guid clusterId,
        ClusterData data,
        [Service] IClusterService clusterService,
        CancellationToken cancellationToken
    )
    {
        return clusterService.ValidateUpdateAsync(clusterId, data, cancellationToken);
    }

    /// <summary>
    /// Delete a cluster.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="dbContext">Database context.</param>
    /// <param name="clusterService">Cluster service.</param>
    /// <param name="mapper">Mapper.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted cluster resource.</returns>
    public static async Task<ClusterResource> DeleteClusterAsync(
        Guid clusterId,
        K8CloudDbContext dbContext,
        [Service] IClusterService clusterService,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var cluster = await clusterService
            .DeleteAsync(clusterId, cancellationToken)
            .ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return mapper.Map<ClusterResource>(cluster);
    }
}

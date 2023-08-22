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
    [Error(typeof(ValidationException))]
    public static async Task<ClusterResource> CreateClusterAsync(
        ClusterData data,
        K8CloudDbContext dbContext,
        [Service] ClusterService clusterService,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        var cluster = await clusterService.CreateAsync(data, cancellationToken).ConfigureAwait(false);

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return mapper.Map<ClusterResource>(cluster);
    }

    public static Task<FluentValidation.Results.ValidationResult> ValidateCreateClusterAsync(
        ClusterData data,
        [Service] ClusterService clusterService,
        CancellationToken cancellationToken
    )
    {
        return clusterService.ValidateCreateAsync(data, cancellationToken);
    }

    [Error(typeof(ValidationException))]
    public static async Task<ClusterResource> UpdateClusterAsync(
        Guid clusterId,
        ClusterData data,
        string version,
        K8CloudDbContext dbContext,
        [Service] ClusterService clusterService,
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

    public static Task<FluentValidation.Results.ValidationResult> ValidateUpdateClusterAsync(
        Guid clusterId,
        ClusterData data,
        [Service] ClusterService clusterService,
        CancellationToken cancellationToken
    )
    {
        return clusterService.ValidateUpdateAsync(clusterId, data, cancellationToken);
    }

    public static async Task<ClusterResource> DeleteClusterAsync(
        Guid clusterId,
        K8CloudDbContext dbContext,
        [Service] ClusterService clusterService,
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

using FluentValidation.Results;
using K8Cloud.Cluster.Entities;
using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Cluster.Services
{
    internal interface IClusterService
    {
        Task<ClusterEntity> CreateAsync(
            ClusterData data,
            CancellationToken cancellationToken = default
        );
        Task<ClusterEntity> DeleteAsync(
            Guid clusterId,
            CancellationToken cancellationToken = default
        );
        Task<ClusterEntity> UpdateAsync(
            Guid clusterId,
            ClusterData data,
            string version,
            CancellationToken cancellationToken = default
        );
        Task<ValidationResult> ValidateCreateAsync(
            ClusterData data,
            CancellationToken cancellationToken = default
        );
        Task<ValidationResult> ValidateUpdateAsync(
            Guid clusterId,
            ClusterData data,
            CancellationToken cancellationToken = default
        );
    }
}

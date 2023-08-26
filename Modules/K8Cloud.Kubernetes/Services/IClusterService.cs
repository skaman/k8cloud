using FluentValidation.Results;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Entities;

namespace K8Cloud.Kubernetes.Services
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

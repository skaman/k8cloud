using FluentValidation.Internal;
using FluentValidation.Results;
using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Cluster.Validators;

internal interface IClusterDataValidator
{
    Task<ValidationResult> ValidateForCreateAsync(
        ClusterData data,
        Action<ValidationStrategy<ClusterData>>? options = null,
        CancellationToken cancellationToken = default
    );
    Task<ValidationResult> ValidateForUpdateAsync(
        Guid clusterId,
        ClusterData data,
        Action<ValidationStrategy<ClusterData>>? options = null,
        CancellationToken cancellationToken = default
    );
}

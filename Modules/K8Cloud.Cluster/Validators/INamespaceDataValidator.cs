using FluentValidation.Internal;
using FluentValidation.Results;
using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Cluster.Validators;

internal interface INamespaceDataValidator
{
    Task<ValidationResult> ValidateForCreateAsync(
        Guid clusterId,
        NamespaceData data,
        Action<ValidationStrategy<NamespaceData>>? options = null,
        CancellationToken cancellationToken = default
    );
    Task<ValidationResult> ValidateForUpdateAsync(
        Guid clusterId,
        Guid namespaceId,
        NamespaceData data,
        Action<ValidationStrategy<NamespaceData>>? options = null,
        CancellationToken cancellationToken = default
    );
}

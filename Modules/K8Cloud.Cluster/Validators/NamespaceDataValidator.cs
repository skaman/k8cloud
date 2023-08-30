using FluentValidation;
using FluentValidation.Internal;
using K8Cloud.Cluster.Extensions;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Shared.Database;
using K8Cloud.Shared.Extensions;
using K8Cloud.Shared.Validators;
using Microsoft.EntityFrameworkCore;

namespace K8Cloud.Cluster.Validators;

/// <summary>
/// Namespace data validator.
/// </summary>
internal class NamespaceDataValidator : AbstractValidator<NamespaceData>, INamespaceDataValidator
{
    public const string NamespaceIdKey = "NamespaceId";
    public const string ClusterIdKey = "ClusterId";

    public NamespaceDataValidator(K8CloudDbContext dbContext)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Rfc1123()
            .CustomAsync(
                async (name, context, cancellationToken) =>
                {
                    var clusterId = context.RootContextData.GetValue<Guid>(ClusterIdKey);
                    var query = dbContext.NamespacesReadOnly().Where(x => x.ClusterId == clusterId);
                    if (context.RootContextData.TryGetValue<Guid>(NamespaceIdKey, out var id))
                    {
                        query = query.Where(x => x.Id != id);
                    }
                    var exists = await query
                        .AnyAsync(x => x.Name == name, cancellationToken)
                        .ConfigureAwait(false);
                    if (exists)
                    {
                        context.AddFailure(
                            nameof(ClusterData.ServerName),
                            $"'{context.DisplayName}' already exists"
                        );
                    }
                }
            );
    }

    /// <summary>
    /// Validate for create a new namespace.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="data">Namespace data.</param>
    /// <param name="options">Callback that allows extra options to be configured.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public Task<FluentValidation.Results.ValidationResult> ValidateForCreateAsync(
        Guid clusterId,
        NamespaceData data,
        Action<ValidationStrategy<NamespaceData>>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var context = ValidationContext<NamespaceData>.CreateWithOptions(
            data,
            o => options?.Invoke(o)
        );
        context.RootContextData.Add(ClusterIdKey, clusterId);
        return ValidateAsync(context, cancellationToken);
    }

    /// <summary>
    /// Validate for update a namespace.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="namespaceId">Namespace ID.</param>
    /// <param name="data">Namespace data.</param>
    /// <param name="options">Callback that allows extra options to be configured.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public Task<FluentValidation.Results.ValidationResult> ValidateForUpdateAsync(
        Guid clusterId,
        Guid namespaceId,
        NamespaceData data,
        Action<ValidationStrategy<NamespaceData>>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var context = ValidationContext<NamespaceData>.CreateWithOptions(
            data,
            o => options?.Invoke(o)
        );
        context.RootContextData.Add(ClusterIdKey, clusterId);
        context.RootContextData.Add(NamespaceIdKey, namespaceId);
        return ValidateAsync(context, cancellationToken);
    }
}

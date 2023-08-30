using FluentValidation;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Shared.Database;
using K8Cloud.Shared.Extensions;
using K8Cloud.Shared.Validators;
using Microsoft.EntityFrameworkCore;
using FluentValidation.Internal;
using K8Cloud.Cluster.Extensions;

namespace K8Cloud.Cluster.Validators;

/// <summary>
/// Cluster data validator.
/// </summary>
internal class ClusterDataValidator : AbstractValidator<ClusterData>, IClusterDataValidator
{
    private const string ClusterIdKey = "ClusterId";

    public ClusterDataValidator(K8CloudDbContext dbContext)
    {
        RuleFor(x => x.ServerName)
            .NotEmpty()
            .Rfc1123()
            .CustomAsync(
                async (serverName, context, cancellationToken) =>
                {
                    var query = dbContext.ClustersReadOnly();
                    if (context.RootContextData.TryGetValue<Guid>(ClusterIdKey, out var id))
                    {
                        query = query.Where(x => x.Id != id);
                    }
                    var exists = await query
                        .AnyAsync(x => x.ServerName == serverName, cancellationToken)
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
        RuleFor(x => x.ServerAddress).NotEmpty().Url(onlyHttps: true);
        RuleFor(x => x.ServerCertificateAuthorityData).NotEmpty().Base64();
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.UserCredentialsCertificateData).NotEmpty().Base64();
        RuleFor(x => x.UserCredentialsKeyData).NotEmpty().Base64();
        RuleFor(x => x.Namespace).NotEmpty();
    }

    /// <summary>
    /// Validate for create a new cluster.
    /// </summary>
    /// <param name="data">Cluster data.</param>
    /// <param name="options">Callback that allows extra options to be configured.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public Task<FluentValidation.Results.ValidationResult> ValidateForCreateAsync(
        ClusterData data,
        Action<ValidationStrategy<ClusterData>>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var context = ValidationContext<ClusterData>.CreateWithOptions(
            data,
            o => options?.Invoke(o)
        );
        return ValidateAsync(context, cancellationToken);
    }

    /// <summary>
    /// Validate for update a cluster.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="data">Cluster data.</param>
    /// <param name="options">Callback that allows extra options to be configured.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public Task<FluentValidation.Results.ValidationResult> ValidateForUpdateAsync(
        Guid clusterId,
        ClusterData data,
        Action<ValidationStrategy<ClusterData>>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var context = ValidationContext<ClusterData>.CreateWithOptions(
            data,
            o => options?.Invoke(o)
        );
        context.RootContextData.Add(ClusterIdKey, clusterId);
        return ValidateAsync(context, cancellationToken);
    }
}

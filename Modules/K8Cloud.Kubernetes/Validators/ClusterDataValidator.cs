using FluentValidation;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Database;
using K8Cloud.Shared.Database;
using K8Cloud.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace K8Cloud.Kubernetes.Validators;

internal class ClusterDataValidator : AbstractValidator<ClusterData>
{
    public const string IdKey = "Id";

    public ClusterDataValidator(K8CloudDbContext dbContext)
    {
        RuleFor(x => x.ServerName)
            .NotEmpty()
            .CustomAsync(
                async (serverName, context, cancellationToken) =>
                {
                    var query = dbContext.ClustersReadOnly();
                    if (context.RootContextData.TryGetValue<Guid>(IdKey, out var id))
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
                            "Server name already exists"
                        );
                    }
                }
            );
        RuleFor(x => x.ServerAddress)
            .NotEmpty()
            .Custom(
                (serverAddress, context) =>
                {
                    var isValid =
                        Uri.TryCreate(serverAddress, UriKind.Absolute, out var uriResult)
                        && uriResult.Scheme == Uri.UriSchemeHttps;
                    if (!isValid)
                    {
                        context.AddFailure(
                            nameof(ClusterData.ServerAddress),
                            "Server address must be a valid HTTPS URL"
                        );
                    }
                }
            );
        RuleFor(x => x.ServerCertificateAuthorityData).NotEmpty();
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.UserCredentialsCertificateData).NotEmpty();
        RuleFor(x => x.UserCredentialsKeyData).NotEmpty();
        RuleFor(x => x.Namespace).NotEmpty();
    }
}

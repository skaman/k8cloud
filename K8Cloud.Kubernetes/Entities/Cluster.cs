using K8Cloud.Shared.Database;

namespace K8Cloud.Kubernetes.Entities;

internal class Cluster : Entity
{
    public required string ServerName { get; init; }
    public required string ServerAddress { get; init; }
    public required string ServerCertificateAuthorityData { get; init; }
    public required string UserName { get; init; }
    public required string UserCredentialsCertificateData { get; init; }
    public required string UserCredentialsKeyData { get; init; }
    public required string Namespace { get; init; }
}

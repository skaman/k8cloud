using K8Cloud.Shared.Database;

namespace K8Cloud.Kubernetes.Entities;

internal class Cluster : Entity
{
    public required string ServerName { get; set; }
    public required string ServerAddress { get; set; }
    public required string ServerCertificateAuthorityData { get; set; }
    public required string UserName { get; set; }
    public required string UserCredentialsCertificateData { get; set; }
    public required string UserCredentialsKeyData { get; set; }
    public required string Namespace { get; set; }
}

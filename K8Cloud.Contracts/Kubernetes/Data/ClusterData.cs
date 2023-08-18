namespace K8Cloud.Contracts.Kubernetes.Data;

public record ClusterData
{
    public required string ServerName { get; init; }
    public required string ServerAddress { get; init; }
    public required string ServerCertificateAuthorityData { get; init; }
    public required string UserName { get; init; }
    public required string UserCredentialsCertificateData { get; init; }
    public required string UserCredentialsKeyData { get; init; }
    public required string Namespace { get; init; }
}

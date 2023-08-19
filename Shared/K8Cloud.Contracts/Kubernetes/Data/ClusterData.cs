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

public record ClusterRecord : ClusterData
{
    public required Guid Id { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required string Version { get; init; }
}

namespace K8Cloud.Contracts.Kubernetes.Data;

/// <summary>
/// Cluster data.
/// </summary>
public record ClusterData
{
    /// <summary>
    /// Cluster name.
    /// </summary>
    public required string ServerName { get; init; }

    /// <summary>
    /// Cluster address.
    /// </summary>
    public required string ServerAddress { get; init; }

    /// <summary>
    /// Cluster certificate authority data.
    /// </summary>
    public required string ServerCertificateAuthorityData { get; init; }

    /// <summary>
    /// Login username.
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    /// Login user credentials certificate data.
    /// </summary>
    public required string UserCredentialsCertificateData { get; init; }

    /// <summary>
    /// Login user credentials key data.
    /// </summary>
    public required string UserCredentialsKeyData { get; init; }

    /// <summary>
    /// Namespace.
    /// </summary>
    public required string Namespace { get; init; }
}

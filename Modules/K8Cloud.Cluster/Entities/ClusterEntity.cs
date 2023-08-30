using K8Cloud.Shared.Database;

namespace K8Cloud.Cluster.Entities;

/// <summary>
/// Cluster entity.
/// </summary>
internal class ClusterEntity : Entity
{
    /// <summary>
    /// Cluster name.
    /// </summary>
    public required string ServerName { get; set; }

    /// <summary>
    /// Cluster address.
    /// </summary>
    public required string ServerAddress { get; set; }

    /// <summary>
    /// Cluster certificate authority data.
    /// </summary>
    public required string ServerCertificateAuthorityData { get; set; }

    /// <summary>
    /// Login username.
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// Login user credentials certificate data.
    /// </summary>
    public required string UserCredentialsCertificateData { get; set; }

    /// <summary>
    /// Login user credentials key data.
    /// </summary>
    public required string UserCredentialsKeyData { get; set; }

    /// <summary>
    /// Namespace.
    /// </summary>
    public required string Namespace { get; set; }
}

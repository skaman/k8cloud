using K8Cloud.Shared.Database;

namespace K8Cloud.Kubernetes.Entities;

/// <summary>
/// Namespace entity.
/// </summary>
internal class NamespaceEntity : Entity
{
    /// <summary>
    /// Cluster ID where the namespace belongs to.
    /// </summary>
    public required Guid ClusterId { get; set; }

    /// <summary>
    /// Namespace name.
    /// </summary>
    public required string Name { get; set; }
}

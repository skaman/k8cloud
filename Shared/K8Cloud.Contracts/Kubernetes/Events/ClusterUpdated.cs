using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.Events;

/// <summary>
/// Cluster updated event.
/// </summary>
public record ClusterUpdated
{
    /// <summary>
    /// Cluster ID.
    /// </summary>
    public required Guid ClusterId { get; init; }

    /// <summary>
    /// Cluster resource.
    /// </summary>
    public required ClusterResource Resource { get; init; }
}

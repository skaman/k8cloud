using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.Events;

/// <summary>
/// Cluster deleted event.
/// </summary>
public record ClusterDeleted
{
    /// <summary>
    /// Cluster resource.
    /// </summary>
    public required ClusterResource Resource { get; init; }

    /// <summary>
    /// Event timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }
}

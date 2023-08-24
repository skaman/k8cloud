using K8Cloud.Contracts.Interfaces;
using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.Events;

/// <summary>
/// Cluster deleted event.
/// </summary>
public record ClusterDeleted : IEventWithResource<ClusterResource>, ITimestamp
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

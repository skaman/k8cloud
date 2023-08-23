using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.Events;

/// <summary>
/// Namespace sync request event.
/// </summary>
public record NamespaceSync
{
    /// <summary>
    /// Namespace resource.
    /// </summary>
    public required NamespaceResource Resource { get; init; }

    /// <summary>
    /// Event timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }
}

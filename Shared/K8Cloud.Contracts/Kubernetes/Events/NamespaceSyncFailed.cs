using K8Cloud.Contracts.Interfaces;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Enums;

namespace K8Cloud.Contracts.Kubernetes.Events;

/// <summary>
/// Namespace sync error event.
/// </summary>
public record NamespaceSyncFailed : IEventWithResource<NamespaceResource>, ITimestamp
{
    /// <summary>
    /// Deploy type.
    /// </summary>
    public required DeployType DeployType { get; init; }

    /// <summary>
    /// Namespace resource.
    /// </summary>
    public required NamespaceResource Resource { get; init; }

    /// <summary>
    /// Last error message.
    /// </summary>
    public required Status Status { get; init; }

    /// <summary>
    /// Event timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }
}

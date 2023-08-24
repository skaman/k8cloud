using K8Cloud.Contracts.Interfaces;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Enums;

namespace K8Cloud.Contracts.Kubernetes.Events;

/// <summary>
/// Namespace sync request event.
/// </summary>
public record NamespaceSync : IEventWithResource<NamespaceResource>, ITimestamp
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
    /// Event timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }
}

/// <summary>
/// Namespace sync retry request event.
/// </summary>
public record NamespaceSyncRetry : IEventWithResource<NamespaceResource>, ITimestamp
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
    /// Event timestamp.
    /// </summary>
    public required DateTime Timestamp { get; init; }
}

using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.Events;

/// <summary>
/// Namespace updated event.
/// </summary>
public record NamespaceUpdated
{
    /// <summary>
    /// Namespace ID.
    /// </summary>
    public required Guid NamespaceId { get; init; }

    /// <summary>
    /// Namespace resource.
    /// </summary>
    public required NamespaceResource Resource { get; init; }
}

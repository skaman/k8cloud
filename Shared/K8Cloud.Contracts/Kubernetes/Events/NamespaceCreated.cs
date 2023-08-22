using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.Events;

/// <summary>
/// Namespace created event.
/// </summary>
public record NamespaceCreated
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

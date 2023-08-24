using K8Cloud.Contracts.Interfaces;

namespace K8Cloud.Contracts.Kubernetes.Data;

/// <summary>
/// Namespace resource.
/// </summary>
public record NamespaceResource : NamespaceData, IResource
{
    /// <summary>
    /// Resource ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Cluster resource ID.
    /// </summary>
    public required Guid ClusterId { get; init; }

    /// <summary>
    /// Resource created at.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Resource updated at.
    /// </summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Resource version.
    /// </summary>
    public required string Version { get; init; }
}

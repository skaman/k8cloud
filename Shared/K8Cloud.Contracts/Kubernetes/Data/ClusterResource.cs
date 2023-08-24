using K8Cloud.Contracts.Interfaces;

namespace K8Cloud.Contracts.Kubernetes.Data;

/// <summary>
/// Cluster resource.
/// </summary>
public record ClusterResource : ClusterData, IResource
{
    /// <summary>
    /// Resource ID.
    /// </summary>
    public required Guid Id { get; init; }

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

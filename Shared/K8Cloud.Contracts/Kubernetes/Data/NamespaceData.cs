namespace K8Cloud.Contracts.Kubernetes.Data;

/// <summary>
/// Namespace data.
/// </summary>
public record NamespaceData
{
    /// <summary>
    /// Namespace name.
    /// </summary>
    public required string Name { get; init; }
}

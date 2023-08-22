namespace K8Cloud.Contracts.Kubernetes.Data;

public record ClusterResource : ClusterData
{
    public required Guid Id { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public required string Version { get; init; }
}

namespace K8Cloud.Contracts.Kubernetes.Data;

public record ClusterSummary
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }
    public required bool IsOperative { get; init; }
}

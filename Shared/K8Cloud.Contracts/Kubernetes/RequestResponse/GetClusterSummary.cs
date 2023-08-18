namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record GetClusterSummary
{
    public required Guid ClusterId { get; init; }
}

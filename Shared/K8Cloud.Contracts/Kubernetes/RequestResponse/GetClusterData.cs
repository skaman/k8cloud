namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record GetClusterData
{
    public required Guid ClusterId { get; init; }
}

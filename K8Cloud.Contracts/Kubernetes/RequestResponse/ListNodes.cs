namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record ListNodes
{
    public required Guid ClusterId { get; init; }
}

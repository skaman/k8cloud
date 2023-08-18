namespace K8Cloud.Contracts.Kubernetes.Events;

public sealed record RequestClusterStatus
{
    public required Guid ClusterId { get; init; }
}

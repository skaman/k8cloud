using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.Events;

public record UpdatedCluster
{
    public required Guid ClusterId { get; init; }
    public required ClusterData Data { get; init; }
}

using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.Events;

public record DeletedCluster
{
    public required Guid ClusterId { get; init; }
    public required ClusterResource Data { get; init; }
}

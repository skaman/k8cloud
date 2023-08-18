using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record AddCluster
{
    public required Guid Id { get; init; }
    public required ClusterData Data { get; init; }
}

using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record EditCluster
{
    public required Guid Id { get; init; }
    public required ClusterData Data { get; init; }
}

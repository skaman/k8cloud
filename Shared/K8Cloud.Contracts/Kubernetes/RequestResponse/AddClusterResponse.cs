using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record AddClusterResponse
{
    public required Guid ClusterId { get; init; }
    public required ClusterData Data { get; init; }
}

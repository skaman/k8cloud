using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record ListNodesResponse
{
    public required Guid ClusterId { get; init; }
    public required NodeInfo[] Nodes { get; init; }
}

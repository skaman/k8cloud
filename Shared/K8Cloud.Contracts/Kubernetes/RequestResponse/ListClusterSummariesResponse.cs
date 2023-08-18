using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record ListClusterSummariesResponse
{
    public required ClusterSummary[] Items { get; init; }
}

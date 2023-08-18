using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record GetClusterSummaryResponse
{
    public required ClusterSummary Data { get; init; }
}

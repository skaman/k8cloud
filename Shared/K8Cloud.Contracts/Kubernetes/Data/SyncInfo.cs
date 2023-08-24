using K8Cloud.Contracts.Kubernetes.Enums;

namespace K8Cloud.Contracts.Kubernetes.Data;

public record SyncInfo
{
    public required SyncStatus Status { get; init; }
    public required Status? ErrorStatus { get; init; }
}

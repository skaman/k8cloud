using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record ValidateResponse
{
    public bool IsValid { get; init; }
    public required ValidationError[] Errors { get; init; }
}

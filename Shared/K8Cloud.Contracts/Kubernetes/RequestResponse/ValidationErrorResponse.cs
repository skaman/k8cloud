using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record ValidationErrorResponse
{
    public required ValidationError[] Errors { get; init; }
}

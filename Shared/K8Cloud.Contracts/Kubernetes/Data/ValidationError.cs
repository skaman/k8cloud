namespace K8Cloud.Contracts.Kubernetes.Data;

public record ValidationError
{
    public required string Message { get; init; }
    public required string PropertyName { get; init; }
}

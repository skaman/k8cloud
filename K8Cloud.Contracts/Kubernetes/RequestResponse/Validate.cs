namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record Validate<T>
{
    public required T Data { get; init; }
    public required string PropertyName { get; init; }
}

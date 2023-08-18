namespace K8Cloud.Contracts.Kubernetes.RequestResponse;

public record ValidateProperty<T>
{
    public required T Data { get; init; }
    public required string PropertyName { get; init; }
}

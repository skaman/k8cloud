namespace K8Cloud.Contracts.Kubernetes.Data;

public record NodeCondition
{
    public required string Type { get; init; }
    public required bool IsOperative { get; init; }
    public string? Message { get; init; }
}

namespace K8Cloud.Contracts.Kubernetes.Data;

/// <summary>
/// Node condition.
/// </summary>
/// <remarks>
/// Data comes from https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.26/#nodecondition-v1-core
/// </remarks>
public record NodeCondition
{
    /// <summary>
    /// Condition type.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Condition status.
    /// </summary>
    public required bool IsOperative { get; init; }

    /// <summary>
    /// Condition message.
    /// </summary>
    public string? Message { get; init; }
}

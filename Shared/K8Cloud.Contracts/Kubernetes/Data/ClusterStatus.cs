namespace K8Cloud.Contracts.Kubernetes.Data;

/// <summary>
/// Represents the status of a cluster.
/// </summary>
public record ClusterStatus
{
    /// <summary>
    /// The cluster is ready.
    /// </summary>
    public required bool IsOperative { get; init; }

    /// <summary>
    /// Informations about each node of the cluster.
    /// </summary>
    public required NodeInfo[] Nodes { get; init; }
}

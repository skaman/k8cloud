namespace K8Cloud.Contracts.Kubernetes.Data;

public record NodeInfo
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string[] IPAddresses { get; init; }
    public required string[] Roles { get; init; }
    public required NodeCondition[] Conditions { get; init; }

    public string? Architecture { get; init; }
    public string? BootID { get; init; }
    public string? ContainerRuntimeVersion { get; init; }
    public string? KernelVersion { get; init; }
    public string? KubeProxyVersion { get; init; }
    public string? KubeletVersion { get; init; }
    public string? MachineID { get; init; }
    public string? OperatingSystem { get; init; }
    public string? OsImage { get; init; }
    public string? SystemUUID { get; init; }

    public string? PodCIDR { get; init; }
    public required string[] PodCIDRs { get; init; }
}

public record ClusterStatus
{
    public required bool IsOperative { get; init; }
    public required NodeInfo[] Nodes { get; init; }
}

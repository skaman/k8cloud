namespace K8Cloud.Contracts.Kubernetes.Data;

/// <summary>
/// Node informations.
/// </summary>
/// <remarks>
/// Data comes from https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.26/#node-v1-core
/// </remarks>
public record NodeInfo
{
    /// <summary>
    /// Kubernetes node ID.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Node name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Node IP addresses.
    /// </summary>
    public required string[] IPAddresses { get; init; }

    /// <summary>
    /// Node roles.
    /// </summary>
    public required string[] Roles { get; init; }

    /// <summary>
    /// Node conditions.
    /// </summary>
    public required NodeCondition[] Conditions { get; init; }

    /// <summary>
    /// The Architecture reported by the node.
    /// </summary>
    public string? Architecture { get; init; }

    /// <summary>
    /// The Architecture reported by the node.
    /// </summary>
    public string? BootID { get; init; }

    /// <summary>
    /// ContainerRuntime Version reported by the node through runtime remote API (e.g. containerd://1.4.2).
    /// </summary>
    public string? ContainerRuntimeVersion { get; init; }

    /// <summary>
    /// ContainerRuntime Version reported by the node through runtime remote API (e.g. containerd://1.4.2).
    /// </summary>
    public string? KernelVersion { get; init; }

    /// <summary>
    /// ContainerRuntime Version reported by the node through runtime remote API (e.g. containerd://1.4.2).
    /// </summary>
    public string? KubeProxyVersion { get; init; }

    /// <summary>
    /// Kubelet Version reported by the node.
    /// </summary>
    public string? KubeletVersion { get; init; }

    /// <summary>
    /// MachineID reported by the node. For unique machine identification in the cluster this field is preferred.
    /// Learn more from man(5) machine-id: http://man7.org/linux/man-pages/man5/machine-id.5.html.
    /// </summary>
    public string? MachineID { get; init; }

    /// <summary>
    /// The Operating System reported by the node.
    /// </summary>
    public string? OperatingSystem { get; init; }

    /// <summary>
    /// OS Image reported by the node from /etc/os-release (e.g. Debian GNU/Linux 7 (wheezy)).
    /// </summary>
    public string? OsImage { get; init; }

    /// <summary>
    /// SystemUUID reported by the node. For unique machine identification MachineID is preferred.
    /// This field is specific to Red Hat hosts https://access.redhat.com/documentation/en-us/red_hat_subscription_management/1/html/rhsm/uuid
    /// </summary>
    public string? SystemUUID { get; init; }

    /// <summary>
    /// PodCIDR represents the pod IP range assigned to the node.
    /// </summary>
    public string? PodCIDR { get; init; }

    /// <summary>
    /// podCIDRs represents the IP ranges assigned to the node for usage by Pods on that node.
    /// If this field is specified, the 0th entry must match the podCIDR field. It may contain at most 1 value for
    /// each of IPv4 and IPv6.
    /// </summary>
    public required string[] PodCIDRs { get; init; }
}

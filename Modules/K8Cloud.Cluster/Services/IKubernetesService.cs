using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Cluster.Services;

internal interface IKubernetesService
{
    Task CreateNamespaceAsync(NamespaceResource resource);
    Task<NamespaceResource> GetNamespaceAsync(Guid clusterId, string name);
    Task<ClusterStatus> GetStatusAsync(
        Guid clusterId,
        CancellationToken cancellationToken = default
    );
    Task UpdateNamespaceAsync(NamespaceResource resource);
}

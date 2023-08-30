namespace K8Cloud.Cluster.Services;

internal interface IKubernetesClientsService
{
    k8s.Kubernetes GetClient(Guid clusterId);
}

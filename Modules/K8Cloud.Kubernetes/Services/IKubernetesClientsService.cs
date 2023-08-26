namespace K8Cloud.Kubernetes.Services;

internal interface IKubernetesClientsService
{
    k8s.Kubernetes GetClient(Guid clusterId);
}

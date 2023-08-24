using K8Cloud.Contracts.Kubernetes.Enums;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Kubernetes.Exceptions;
using K8Cloud.Kubernetes.Services;
using MassTransit;

namespace K8Cloud.Kubernetes.Consumers;

internal class NamespaceDeployConsumer : IConsumer<NamespaceDeploy>
{
    private readonly KubernetesService _kubernetesService;

    public NamespaceDeployConsumer(KubernetesService kubernetesService)
    {
        _kubernetesService = kubernetesService;
    }

    public async Task Consume(ConsumeContext<NamespaceDeploy> context)
    {
        try
        {
            switch (context.Message.DeployType)
            {
                case DeployType.Apply:
                    await _kubernetesService
                        .CreateOrUpdateNamespaceAsync(context.Message.Resource)
                        .ConfigureAwait(false);
                    break;
                case DeployType.Delete:
                    break;
            }

            await context
                .Publish(
                    new NamespaceDeployCompleted
                    {
                        DeployType = context.Message.DeployType,
                        Resource = context.Message.Resource,
                        Timestamp = DateTime.UtcNow
                    },
                    context.CancellationToken
                )
                .ConfigureAwait(false);
        }
        catch (KubernetesException ex)
        {
            await context
                .Publish(
                    new NamespaceDeployFailed
                    {
                        DeployType = context.Message.DeployType,
                        Resource = context.Message.Resource,
                        Status = ex.Status,
                        Timestamp = DateTime.UtcNow
                    },
                    context.CancellationToken
                )
                .ConfigureAwait(false);
        }
    }
}

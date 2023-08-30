using K8Cloud.Cluster.Exceptions;
using K8Cloud.Cluster.Services;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Enums;
using K8Cloud.Contracts.Kubernetes.Events;
using MassTransit;
using System.Net;

namespace K8Cloud.Cluster.Consumers;

internal class NamespaceDeployConsumer : IConsumer<NamespaceDeploy>
{
    private readonly IKubernetesService _kubernetesService;

    public NamespaceDeployConsumer(IKubernetesService kubernetesService)
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
                    await CreateOrUpdateNamespaceAsync(context.Message.Resource)
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

    private async Task CreateOrUpdateNamespaceAsync(NamespaceResource resource)
    {
        // check existing resource for existence and compatibility
        var needCreate = false;
        try
        {
            var existingResource = await _kubernetesService
                .GetNamespaceAsync(resource.ClusterId, resource.Name)
                .ConfigureAwait(false);

            if (existingResource.Id != resource.Id)
            {
                throw new KubernetesException(
                    new Status
                    {
                        Code = HttpStatusCode.Conflict,
                        Message = "Namespace already exists"
                    }
                );
            }
        }
        catch (KubernetesException e)
        {
            if (e.Status.Code != HttpStatusCode.NotFound)
            {
                throw;
            }

            needCreate = true;
        }

        if (needCreate)
        {
            await _kubernetesService.CreateNamespaceAsync(resource).ConfigureAwait(false);
        }
        else
        {
            await _kubernetesService.UpdateNamespaceAsync(resource).ConfigureAwait(false);
        }
    }
}

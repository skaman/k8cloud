using K8Cloud.Contracts.Kubernetes.Enums;
using K8Cloud.Contracts.Kubernetes.Events;
using MassTransit;

namespace K8Cloud.Kubernetes.Consumers;

internal class NamespaceSyncBridgeConsumer
    : IConsumer<NamespaceCreated>,
        IConsumer<NamespaceUpdated>,
        IConsumer<NamespaceDeleted>
{
    public Task Consume(ConsumeContext<NamespaceCreated> context)
    {
        return context.Publish(
            new NamespaceSync
            {
                DeployType = DeployType.Apply,
                Resource = context.Message.Resource,
                Timestamp = DateTime.UtcNow
            },
            context.CancellationToken
        );
    }

    public Task Consume(ConsumeContext<NamespaceUpdated> context)
    {
        return context.Publish(
            new NamespaceSync
            {
                DeployType = DeployType.Apply,
                Resource = context.Message.Resource,
                Timestamp = DateTime.UtcNow
            },
            context.CancellationToken
        );
    }

    public Task Consume(ConsumeContext<NamespaceDeleted> context)
    {
        return context.Publish(
            new NamespaceSync
            {
                DeployType = DeployType.Delete,
                Resource = context.Message.Resource,
                Timestamp = DateTime.UtcNow
            },
            context.CancellationToken
        );
    }
}

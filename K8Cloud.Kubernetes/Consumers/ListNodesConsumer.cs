using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Contracts.Kubernetes.RequestResponse;
using K8Cloud.Kubernetes.Services;
using k8s;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace K8Cloud.Kubernetes.Consumers;

internal class ListNodesConsumer : IConsumer<ListNodes>
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly KubernetesClientsService _kubernetesClientsService;

    public ListNodesConsumer(
        ILogger<ListNodesConsumer> logger,
        IMapper mapper,
        KubernetesClientsService kubernetesClientsService
    )
    {
        _logger = logger;
        _mapper = mapper;
        _kubernetesClientsService = kubernetesClientsService;
    }

    public async Task Consume(ConsumeContext<ListNodes> context)
    {
        _logger.LogInformation("List nodes in cluster {ClusterId}", context.Message.ClusterId);

        var client = _kubernetesClientsService.GetClient(context.Message.ClusterId);
        var nodes = await client.CoreV1.ListNodeAsync().ConfigureAwait(false);
        await context
            .RespondAsync(
                new ListNodesResponse
                {
                    ClusterId = context.Message.ClusterId,
                    Nodes = _mapper.Map<NodeInfo[]>(nodes.Items)
                }
            )
            .ConfigureAwait(false);
    }
}

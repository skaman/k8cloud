using AutoMapper;
using AutoMapper.QueryableExtensions;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Contracts.Kubernetes.RequestResponse;
using K8Cloud.Kubernetes.Database;
using K8Cloud.Shared.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace K8Cloud.Kubernetes.Consumers;

internal class GetClusterDataConsumer : IConsumer<GetClusterData>
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly K8CloudDbContext _dbContext;

    public GetClusterDataConsumer(
        ILogger<ListNodesConsumer> logger,
        IMapper mapper,
        K8CloudDbContext dbContext
    )
    {
        _logger = logger;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<GetClusterData> context)
    {
        _logger.LogInformation("Get data for cluster {ClusterId}", context.Message.ClusterId);

        var summary = await _dbContext
            .ClustersReadOnly()
            .Where(x => x.Id == context.Message.ClusterId)
            .ProjectTo<ClusterData>(_mapper.ConfigurationProvider)
            .SingleAsync()
            .ConfigureAwait(false);

        await context
            .RespondAsync(
                new GetClusterDataResponse { ClusterId = context.Message.ClusterId, Data = summary }
            )
            .ConfigureAwait(false);
    }
}

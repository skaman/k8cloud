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

internal class GetClusterSummaryConsumer : IConsumer<GetClusterSummary>
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly K8CloudDbContext _dbContext;

    public GetClusterSummaryConsumer(
        ILogger<ListNodesConsumer> logger,
        IMapper mapper,
        K8CloudDbContext dbContext
    )
    {
        _logger = logger;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<GetClusterSummary> context)
    {
        _logger.LogInformation("Get summary for cluster {ClusterId}", context.Message.ClusterId);

        var summary = await _dbContext
            .ClustersReadOnly()
            .ProjectTo<ClusterSummary>(_mapper.ConfigurationProvider)
            .SingleAsync(x => x.Id == context.Message.ClusterId)
            .ConfigureAwait(false);

        await context
            .RespondAsync(new GetClusterSummaryResponse { Data = summary })
            .ConfigureAwait(false);
    }
}

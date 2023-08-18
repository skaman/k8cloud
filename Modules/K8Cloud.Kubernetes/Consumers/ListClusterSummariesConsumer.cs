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

internal class ListClusterSummariesConsumer : IConsumer<ListClusterSummaries>
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly K8CloudDbContext _dbContext;

    public ListClusterSummariesConsumer(
        ILogger<ListNodesConsumer> logger,
        IMapper mapper,
        K8CloudDbContext dbContext
    )
    {
        _logger = logger;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<ListClusterSummaries> context)
    {
        _logger.LogInformation("List cluster summaries");

        var summaries = await _dbContext
            .ClustersReadOnly()
            .ProjectTo<ClusterSummary>(_mapper.ConfigurationProvider)
            .ToArrayAsync()
            .ConfigureAwait(false);

        await context
            .RespondAsync(new ListClusterSummariesResponse { Items = summaries })
            .ConfigureAwait(false);
    }
}

using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.RequestResponse;
using K8Cloud.Kubernetes.Database;
using K8Cloud.Kubernetes.Entities;
using K8Cloud.Kubernetes.Validators;
using K8Cloud.Shared.Consumers;
using K8Cloud.Shared.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace K8Cloud.Kubernetes.Consumers;

internal class AddClusterConsumer : ConsumerWithValidator<AddCluster>
{
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private readonly K8CloudDbContext _dbContext;

    public AddClusterConsumer(
        ILogger<AddClusterConsumer> logger,
        IMapper mapper,
        K8CloudDbContext dbContext,
        AddClusterValidator validator
    ) : base(validator)
    {
        _logger = logger;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public override async Task ConsumeValidated(ConsumeContext<AddCluster> context)
    {
        _logger.LogInformation("Create cluster {ClusterName}", context.Message.Data.ServerName);

        var cluster = await _dbContext
            .ClustersReadOnly()
            .FirstOrDefaultAsync(x => x.Id == context.Message.Id)
            .ConfigureAwait(false);

        if (cluster == null)
        {
            cluster = _mapper.Map<Cluster>(context.Message);
            await _dbContext.AddAsync(cluster, context.CancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);
        }

        await context
            .RespondAsync(
                new AddClusterResponse { ClusterId = cluster.Id, Data = _mapper.Map<ClusterData>(cluster) }
            )
            .ConfigureAwait(false);
    }
}

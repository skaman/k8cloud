using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Kubernetes.Entities;
using K8Cloud.Kubernetes.Extensions;
using K8Cloud.Kubernetes.Validators;
using K8Cloud.Shared.Database;
using k8s;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace K8Cloud.Kubernetes.Services;

/// <summary>
/// Manages the clusters.
/// </summary>
internal class ClusterService
{
    private static TimeSpan CacheExpiration = TimeSpan.FromSeconds(30);

    private readonly K8CloudDbContext _dbContext;
    private readonly ClusterDataValidator _clusterDataValidator;
    private readonly KubernetesClientsService _kubernetesClientsService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;

    public ClusterService(
        K8CloudDbContext dbContext,
        ClusterDataValidator clusterDataValidator,
        KubernetesClientsService kubernetesClientsService,
        IPublishEndpoint publishEndpoint,
        IMapper mapper,
        IDistributedCache cache
    )
    {
        _dbContext = dbContext;
        _clusterDataValidator = clusterDataValidator;
        _kubernetesClientsService = kubernetesClientsService;
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
        _cache = cache;
    }

    /// <summary>
    /// Creates a new cluster.
    /// </summary>
    /// <param name="data">Cluster data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created cluster entity.</returns>
    public async Task<ClusterEntity> CreateAsync(
        ClusterData data,
        CancellationToken cancellationToken = default
    )
    {
        // validate the data
        await _clusterDataValidator
            .ValidateForCreateAsync(
                data,
                options: o => o.ThrowOnFailures(),
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        // create and add the cluster
        var cluster = _mapper.Map<ClusterEntity>(data);
        cluster.Id = NewId.NextGuid();
        await _dbContext.AddAsync(cluster, cancellationToken).ConfigureAwait(false);

        // publish the event
        await _publishEndpoint
            .Publish(
                new ClusterCreated
                {
                    ClusterId = cluster.Id,
                    Resource = _mapper.Map<ClusterResource>(cluster)
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // save the changes
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return cluster;
    }

    /// <summary>
    /// Validates the cluster data for creation.
    /// </summary>
    /// <param name="data">Cluster data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public Task<FluentValidation.Results.ValidationResult> ValidateCreateAsync(
        ClusterData data,
        CancellationToken cancellationToken = default
    )
    {
        return _clusterDataValidator.ValidateForCreateAsync(
            data,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Updates data of the cluster.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="data">Data to update.</param>
    /// <param name="version">Resource version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated cluster entity.</returns>
    public async Task<ClusterEntity> UpdateAsync(
        Guid clusterId,
        ClusterData data,
        string version,
        CancellationToken cancellationToken = default
    )
    {
        // validate the data
        await _clusterDataValidator
            .ValidateForUpdateAsync(
                clusterId,
                data,
                options: o => o.ThrowOnFailures(),
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);

        // update the cluster
        var cluster = await _dbContext
            .Clusters()
            .SingleAsync(x => x.Id == clusterId, cancellationToken)
            .ConfigureAwait(false);
        _mapper.Map(data, cluster);
        cluster.Version = uint.Parse(version);

        _dbContext.Update(cluster);

        // publish the event
        await _publishEndpoint
            .Publish(
                new ClusterUpdated
                {
                    ClusterId = cluster.Id,
                    Resource = _mapper.Map<ClusterResource>(cluster)
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // save the changes
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return cluster;
    }

    /// <summary>
    /// Validates the cluster data for update.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="data">Data to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    public Task<FluentValidation.Results.ValidationResult> ValidateUpdateAsync(
        Guid clusterId,
        ClusterData data,
        CancellationToken cancellationToken = default
    )
    {
        return _clusterDataValidator.ValidateForUpdateAsync(
            clusterId,
            data,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Deletes the cluster.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted cluster entity.</returns>
    public async Task<ClusterEntity> DeleteAsync(
        Guid clusterId,
        CancellationToken cancellationToken = default
    )
    {
        // delete the cluster
        var cluster = await _dbContext
            .Clusters()
            .SingleAsync(x => x.Id == clusterId, cancellationToken)
            .ConfigureAwait(false);
        _dbContext.Remove(cluster);

        // publish the event
        await _publishEndpoint
            .Publish(
                new ClusterDeleted
                {
                    ClusterId = cluster.Id,
                    Resource = _mapper.Map<ClusterResource>(cluster)
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        // save the changes
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return cluster;
    }

    /// <summary>
    /// Get the status of the cluster.
    /// The status is requested from the cluster and cached for 30 seconds.
    /// </summary>
    /// <param name="clusterId">Cluster ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cluster status.</returns>
    public async Task<ClusterResourceStatus> GetStatusAsync(
        Guid clusterId,
        CancellationToken cancellationToken = default
    )
    {
        // try to get the status from the cache
        var key = $"clusterStatus:{clusterId}";
        var cacheValue = await _cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(cacheValue))
        {
            var deserializedData = JsonSerializer.Deserialize<ClusterResourceStatus>(cacheValue);
            if (deserializedData != null)
            {
                return deserializedData;
            }
        }

        // get the status from the cluster
        var client = _kubernetesClientsService.GetClient(clusterId);
        var response = await client.CoreV1
            .ListNodeAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var nodes = _mapper.Map<NodeInfo[]>(response.Items);

        var status = new ClusterResourceStatus
        {
            IsOperative = !Array.Exists(
                nodes,
                node =>
                    Array.Exists(
                        node.Conditions,
                        condition => condition.Type == "Ready" && !condition.IsOperative
                    )
            ),
            Nodes = nodes
        };

        // update the cache
        await _cache
            .SetStringAsync(
                key,
                JsonSerializer.Serialize(status),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = CacheExpiration
                },
                cancellationToken
            )
            .ConfigureAwait(false);

        return status;
    }
}

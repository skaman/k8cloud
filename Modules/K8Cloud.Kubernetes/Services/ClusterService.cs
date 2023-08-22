using AutoMapper;
using FluentValidation;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Kubernetes.Database;
using K8Cloud.Kubernetes.Entities;
using K8Cloud.Kubernetes.Validators;
using K8Cloud.Shared.Database;
using k8s;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace K8Cloud.Kubernetes.Services;

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

    public async Task<Cluster> CreateAsync(
        ClusterData data,
        CancellationToken cancellationToken = default
    )
    {
        await _clusterDataValidator
            .ValidateAndThrowAsync(data, cancellationToken)
            .ConfigureAwait(false);
        var cluster = new Cluster
        {
            Id = NewId.NextGuid(),
            ServerName = data.ServerName,
            ServerAddress = data.ServerAddress,
            ServerCertificateAuthorityData = data.ServerCertificateAuthorityData,
            UserName = data.UserName,
            UserCredentialsCertificateData = data.UserCredentialsCertificateData,
            UserCredentialsKeyData = data.UserCredentialsKeyData,
            Namespace = data.Namespace
        };

        await _dbContext.AddAsync(cluster, cancellationToken).ConfigureAwait(false);

        await _publishEndpoint
            .Publish(
                new CreatedCluster
                {
                    ClusterId = cluster.Id,
                    Data = _mapper.Map<ClusterResource>(cluster)
                },
                cancellationToken
            )
            .ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return cluster;
    }

    public Task<FluentValidation.Results.ValidationResult> ValidateCreateAsync(
        ClusterData data,
        CancellationToken cancellationToken = default
    )
    {
        return _clusterDataValidator.ValidateAsync(data, cancellationToken);
    }

    public async Task<Cluster> UpdateAsync(
        Guid clusterId,
        ClusterData data,
        string version,
        CancellationToken cancellationToken = default
    )
    {
        var cluster = await _dbContext
            .ClustersReadOnly()
            .SingleAsync(x => x.Id == clusterId, cancellationToken)
            .ConfigureAwait(false);

        var context = ValidationContext<ClusterData>.CreateWithOptions(
            data,
            options => options.ThrowOnFailures()
        );
        context.RootContextData.Add(ClusterDataValidator.IdKey, clusterId);
        await _clusterDataValidator.ValidateAsync(context, cancellationToken).ConfigureAwait(false);

        cluster.ServerName = data.ServerName;
        cluster.ServerAddress = data.ServerAddress;
        cluster.ServerCertificateAuthorityData = data.ServerCertificateAuthorityData;
        cluster.UserName = data.UserName;
        cluster.UserCredentialsCertificateData = data.UserCredentialsCertificateData;
        cluster.UserCredentialsKeyData = data.UserCredentialsKeyData;
        cluster.Namespace = data.Namespace;
        cluster.Version = uint.Parse(version);

        _dbContext.Update(cluster);

        await _publishEndpoint
            .Publish(
                new UpdatedCluster
                {
                    ClusterId = cluster.Id,
                    Data = _mapper.Map<ClusterResource>(cluster)
                },
                cancellationToken
            )
            .ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return cluster;
    }

    public Task<FluentValidation.Results.ValidationResult> ValidateUpdateAsync(
        Guid clusterId,
        ClusterData data,
        CancellationToken cancellationToken = default
    )
    {
        var context = new ValidationContext<ClusterData>(data);
        context.RootContextData.Add(ClusterDataValidator.IdKey, clusterId);
        return _clusterDataValidator.ValidateAsync(context, cancellationToken);
    }

    public async Task<Cluster> DeleteAsync(
        Guid clusterId,
        CancellationToken cancellationToken = default
    )
    {
        var cluster = await _dbContext
            .ClustersReadOnly()
            .SingleAsync(x => x.Id == clusterId, cancellationToken)
            .ConfigureAwait(false);
        _dbContext.Remove(cluster);

        await _publishEndpoint
            .Publish(
                new DeletedCluster
                {
                    ClusterId = cluster.Id,
                    Data = _mapper.Map<ClusterResource>(cluster)
                },
                cancellationToken
            )
            .ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return cluster;
    }

    public async Task<ClusterStatus> GetStatusAsync(
        Guid clusterId,
        CancellationToken cancellationToken = default
    )
    {
        var key = $"clusterStatus:{clusterId}";
        var cacheValue = await _cache.GetStringAsync(key, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(cacheValue))
        {
            var deserializedData = JsonSerializer.Deserialize<ClusterStatus>(cacheValue);
            if (deserializedData != null)
            {
                return deserializedData;
            }
        }

        var client = _kubernetesClientsService.GetClient(clusterId);
        var response = await client.CoreV1
            .ListNodeAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var nodes = _mapper.Map<NodeInfo[]>(response.Items);

        var status = new ClusterStatus
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

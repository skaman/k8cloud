using AutoMapper;
using FluentValidation;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Kubernetes.Database;
using K8Cloud.Kubernetes.Entities;
using K8Cloud.Kubernetes.Validators;
using K8Cloud.Shared.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using static HotChocolate.ErrorCodes;

namespace K8Cloud.Kubernetes.Services;

internal class ClusterService
{
    private readonly K8CloudDbContext _dbContext;
    private readonly ClusterDataValidator _clusterDataValidator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IMapper _mapper;

    public ClusterService(
        K8CloudDbContext dbContext,
        ClusterDataValidator clusterDataValidator,
        IPublishEndpoint publishEndpoint,
        IMapper mapper
    )
    {
        _dbContext = dbContext;
        _clusterDataValidator = clusterDataValidator;
        _publishEndpoint = publishEndpoint;
        _mapper = mapper;
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
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _publishEndpoint
            .Publish(new CreatedCluster { ClusterId = cluster.Id, Data = data }, cancellationToken)
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
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _publishEndpoint
            .Publish(new UpdatedCluster { ClusterId = cluster.Id, Data = data }, cancellationToken)
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
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _publishEndpoint
            .Publish(
                new DeletedCluster
                {
                    ClusterId = cluster.Id,
                    Data = _mapper.Map<ClusterData>(cluster)
                },
                cancellationToken
            )
            .ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return cluster;
    }
}

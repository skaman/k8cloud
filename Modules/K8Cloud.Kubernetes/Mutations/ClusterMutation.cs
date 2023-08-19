using AutoMapper;
using FluentValidation;
using HotChocolate;
using HotChocolate.Types;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Database;
using K8Cloud.Kubernetes.Entities;
using K8Cloud.Kubernetes.Validators;
using K8Cloud.Shared.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace K8Cloud.Kubernetes.Mutations;

[MutationType]
internal static class ClusterMutation
{
    [Error(typeof(ValidationException))]
    public static async Task<ClusterRecord> CreateClusterAsync(
        ClusterData data,
        K8CloudDbContext dbContext,
        [Service] ClusterDataValidator validator,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        await validator.ValidateAndThrowAsync(data, cancellationToken).ConfigureAwait(false);
        var record = new Cluster
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

        await dbContext.AddAsync(record, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return mapper.Map<ClusterRecord>(record);
    }

    public static async Task<FluentValidation.Results.ValidationResult> ValidateCreateClusterAsync(
        ClusterData data,
        [Service] ClusterDataValidator validator,
        CancellationToken cancellationToken
    )
    {
        return await validator.ValidateAsync(data, cancellationToken).ConfigureAwait(false);
    }

    [Error(typeof(ValidationException))]
    public static async Task<ClusterRecord> UpdateClusterAsync(
        Guid clusterId,
        ClusterData data,
        string version,
        K8CloudDbContext dbContext,
        [Service] ClusterDataValidator validator,
        [Service] IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var record = await dbContext
            .ClustersReadOnly()
            .SingleAsync(x => x.Id == clusterId, cancellationToken)
            .ConfigureAwait(false);

        var context = ValidationContext<ClusterData>.CreateWithOptions(
            data,
            options => options.ThrowOnFailures()
        );
        context.RootContextData.Add(ClusterDataValidator.IdKey, clusterId);
        await validator.ValidateAsync(context, cancellationToken).ConfigureAwait(false);

        record.ServerName = data.ServerName;
        record.ServerAddress = data.ServerAddress;
        record.ServerCertificateAuthorityData = data.ServerCertificateAuthorityData;
        record.UserName = data.UserName;
        record.UserCredentialsCertificateData = data.UserCredentialsCertificateData;
        record.UserCredentialsKeyData = data.UserCredentialsKeyData;
        record.Namespace = data.Namespace;
        record.Version = uint.Parse(version);

        dbContext.Update(record);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return mapper.Map<ClusterRecord>(record);
    }

    public static async Task<FluentValidation.Results.ValidationResult> ValidateUpdateClusterAsync(
        Guid clusterId,
        ClusterData data,
        [Service] ClusterDataValidator validator,
        CancellationToken cancellationToken
    )
    {
        var context = new ValidationContext<ClusterData>(data);
        context.RootContextData.Add(ClusterDataValidator.IdKey, clusterId);
        return await validator.ValidateAsync(context, cancellationToken).ConfigureAwait(false);
    }
}

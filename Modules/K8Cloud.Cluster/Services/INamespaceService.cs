﻿using FluentValidation.Results;
using K8Cloud.Cluster.Entities;
using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Cluster.Services;

internal interface INamespaceService
{
    Task<NamespaceEntity> CreateAsync(
        Guid clusterId,
        NamespaceData data,
        CancellationToken cancellationToken = default
    );
    Task<NamespaceEntity> DeleteAsync(
        Guid clusterId,
        Guid namespaceId,
        CancellationToken cancellationToken = default
    );
    Task<NamespaceEntity> UpdateAsync(
        Guid clusterId,
        Guid namespaceId,
        NamespaceData data,
        string version,
        CancellationToken cancellationToken = default
    );
    Task<ValidationResult> ValidateCreateAsync(
        Guid clusterId,
        NamespaceData data,
        CancellationToken cancellationToken = default
    );
    Task<ValidationResult> ValidateUpdateAsync(
        Guid clusterId,
        Guid namespaceId,
        NamespaceData data,
        CancellationToken cancellationToken = default
    );
}

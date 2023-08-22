using K8Cloud.Kubernetes.Entities;
using K8Cloud.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace K8Cloud.Kubernetes.Extensions;

/// <summary>
/// Database extensions.
/// </summary>
internal static class DatabaseExtensions
{
    /// <summary>
    /// Get the clusters queryable.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <returns>Clusters queryable.</returns>
    public static IQueryable<ClusterEntity> Clusters(this K8CloudDbContext dbContext) =>
        dbContext.Set<ClusterEntity>().AsQueryable();

    /// <summary>
    /// Get the clusters queryable for read-only operations.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <returns>Clusters queryable.</returns>
    public static IQueryable<ClusterEntity> ClustersReadOnly(this K8CloudDbContext dbContext) =>
        dbContext.Set<ClusterEntity>().AsNoTracking().AsQueryable();

    /// <summary>
    /// Get the namespaces queryable.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <returns>Namespaces queryable.</returns>
    public static IQueryable<NamespaceEntity> Namespaces(this K8CloudDbContext dbContext) =>
        dbContext.Set<NamespaceEntity>().AsQueryable();

    /// <summary>
    /// Get the namespaces queryable for read-only operations.
    /// </summary>
    /// <param name="dbContext">Database context.</param>
    /// <returns>Namespaces queryable.</returns>
    public static IQueryable<NamespaceEntity> NamespacesReadOnly(this K8CloudDbContext dbContext) =>
        dbContext.Set<NamespaceEntity>().AsNoTracking().AsQueryable();
}

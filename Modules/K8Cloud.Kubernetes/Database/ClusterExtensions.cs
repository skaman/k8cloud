using K8Cloud.Kubernetes.Entities;
using K8Cloud.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace K8Cloud.Kubernetes.Database;

internal static class ClusterExtensions
{
    public static IQueryable<Cluster> Clusters(this K8CloudDbContext dbContext) =>
        dbContext.Set<Cluster>().AsQueryable();

    public static IQueryable<Cluster> ClustersReadOnly(this K8CloudDbContext dbContext) =>
        dbContext.Set<Cluster>().AsNoTracking().AsQueryable();
}

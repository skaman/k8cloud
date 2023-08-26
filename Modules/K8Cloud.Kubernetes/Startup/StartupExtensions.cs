using AutoMapper;
using K8Cloud.Kubernetes.Consumers;
using K8Cloud.Kubernetes.Entities;
using K8Cloud.Kubernetes.Mappers;
using K8Cloud.Kubernetes.Services;
using K8Cloud.Kubernetes.StateMachines.Namespace;
using K8Cloud.Kubernetes.Validators;
using K8Cloud.Shared.Database;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace K8Cloud.Kubernetes.Startup;

/// <summary>
/// Startup extensions.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Registers the kubernetes module.
    /// </summary>
    /// <param name="services">Services collection.</param>
    public static IServiceCollection AddKubernetesModule(this IServiceCollection services)
    {
        // validators
        services.AddScoped<IClusterDataValidator, ClusterDataValidator>();
        services.AddScoped<NamespaceDataValidator>();

        // other scoped services
        services.AddScoped<KubernetesService>();
        services.AddScoped<IClusterService, ClusterService>();
        services.AddScoped<NamespaceService>();

        // singleton services
        services.AddSingleton<KubernetesClientsService>();

        return services;
    }

    /// <summary>
    /// Configures the bus for the kubernetes module.
    /// </summary>
    /// <param name="busConfigurator">MassTransit bus configurator.</param>
    public static void ConfigureKubernetesBus(this IBusRegistrationConfigurator busConfigurator)
    {
        busConfigurator.AddConsumer<NamespaceDeployConsumer>();
        busConfigurator.AddConsumer<NamespaceSyncBridgeConsumer>();

        busConfigurator
            .AddSagaStateMachine<NamespaceSyncStateMachine, NamespaceSyncState>()
            .EntityFrameworkRepository(r =>
            {
                r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
                r.ExistingDbContext<K8CloudDbContext>();
                r.LockStatementProvider = new PostgresLockStatementProvider();
            });
    }

    /// <summary>
    /// Configure the database for the kubernetes module.
    /// </summary>
    /// <param name="modelBuilder">Entity Framework model builder.</param>
    public static void ConfigureKubernetesTables(this ModelBuilder modelBuilder)
    {
        var clusterEntity = modelBuilder.Entity<ClusterEntity>().ToTable("KubernetsClusters");
        clusterEntity.HasIndex(x => x.ServerName).IsUnique();

        var namespaceEntity = modelBuilder.Entity<NamespaceEntity>().ToTable("KubernetsNamespaces");
        namespaceEntity.HasIndex(x => new { x.Id, x.ClusterId });
        namespaceEntity.HasIndex(x => x.Name).IsUnique();

        var namespaceSyncStateEntity = modelBuilder
            .Entity<NamespaceSyncState>()
            .ToTable("KubernetsNamespaceSyncState");
        namespaceSyncStateEntity.HasKey(x => x.CorrelationId);
        namespaceSyncStateEntity.Property(p => p.InSyncResouce).HasColumnType("jsonb");
        namespaceSyncStateEntity.Property(p => p.SyncedResouce).HasColumnType("jsonb");
        namespaceSyncStateEntity.Property(p => p.ErrorStatus).HasColumnType("jsonb");
    }

    public static void ConfigureKubernetesAutoMapper(this IMapperConfigurationExpression config)
    {
        config.AddProfile<ClusterProfile>();
        config.AddProfile<NamespaceProfile>();
        config.AddProfile<NodeProfile>();
    }
}

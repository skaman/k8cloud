using AutoMapper;
using K8Cloud.Kubernetes.Entities;
using K8Cloud.Kubernetes.Mappers;
using K8Cloud.Kubernetes.Services;
using K8Cloud.Kubernetes.Validators;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    /// <param name="configuration">Configuration.</param>
    public static void AddKubernetesModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        //services.AddScoped<AddClusterValidator>();
        services.AddScoped<ClusterDataValidator>();

        services.AddScoped<ClusterService>();

        services.AddSingleton<KubernetesClientsService>();
    }

    /// <summary>
    /// Configures the bus for the kubernetes module.
    /// </summary>
    /// <param name="busConfigurator">MassTransit bus configurator.</param>
    public static void ConfigureKubernetesBus(this IBusRegistrationConfigurator busConfigurator)
    {
        //busConfigurator.AddConsumer<AddClusterConsumer>();
        //busConfigurator.AddConsumer<ListNodesConsumer>();
        //busConfigurator.AddConsumer<ListClusterSummariesConsumer>();
        //busConfigurator.AddConsumer<GetClusterSummaryConsumer>();
        //busConfigurator.AddConsumer<GetClusterDataConsumer>();
        //busConfigurator
        //    .AddSagaStateMachine<ClusterStateMachine, ClusterState>()
        //    .EntityFrameworkRepository(r =>
        //    {
        //        r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
        //        r.ExistingDbContext<K8CloudDbContext>();
        //        r.LockStatementProvider = new SqliteLockStatementProvider(); // TODO: make it configurable
        //    });
    }

    /// <summary>
    /// Configure the database for the kubernetes module.
    /// </summary>
    /// <param name="modelBuilder">Entity Framework model builder.</param>
    public static void ConfigureKubernetesTables(this ModelBuilder modelBuilder)
    {
        var clusterEntity = modelBuilder.Entity<Cluster>().ToTable("KubernetsClusters");
        clusterEntity.HasIndex(x => x.ServerName).IsUnique();
        //clusterEntity.Property(p => p.Version).IsRowVersion();

        //var sendCommandState = modelBuilder.Entity<ClusterState>().ToTable("KubernetsClusters");
        //sendCommandState.HasKey(x => x.CorrelationId);
        //sendCommandState.Property(x => x.CorrelationId).HasColumnName("Id").ValueGeneratedNever();
        //sendCommandState.Property(x => x.CurrentState).HasMaxLength(64).IsRequired();
    }

    public static void ConfigureKubernetesAutoMapper(this IMapperConfigurationExpression config)
    {
        config.AddProfile<ClusterProfile>();
        config.AddProfile<NodeProfile>();
    }
}

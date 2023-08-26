using K8Cloud.Kubernetes.Startup;
using K8Cloud.Shared.Startup;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Reflection;

namespace K8Cloud.Kubernetes.Tests.Utils;

internal static class ServicesExtensions
{
    public static IServiceCollection AddTestDatabase(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddK8CloudDatabase(
            optionsAction: options =>
            {
                options.UseNpgsql(
                    connectionString,
                    options =>
                    {
                        options.MigrationsAssembly(
                            Assembly
                                // TODO: use a better way to get the assembly name
                                .GetAssembly(typeof(Server.Migrations.InitialCreate))!
                                .GetName()
                                .Name
                        );
                    }
                );
            },
            modelBuilderAction: modelBuilder =>
            {
                modelBuilder.ConfigureKubernetesTables();
            }
        );

        return services;
    }

    public static IServiceCollection AddTestMassTransit(this IServiceCollection services)
    {
        return services
            .AddQuartz(x =>
            {
                x.UseMicrosoftDependencyInjectionJobFactory();
            })
            .AddMassTransitTestHarness(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                x.AddQuartzConsumers();

                x.AddPublishMessageScheduler();

                //configure?.Invoke(x);

                x.UsingInMemory(
                    (context, cfg) =>
                    {
                        cfg.UsePublishMessageScheduler();

                        cfg.ConfigureEndpoints(context);
                    }
                );
            });
    }

    public static IServiceCollection ConfigureForKubernetesModule(
        this IServiceCollection services,
        string connectionString
    )
    {
        services
            .AddTestDatabase(connectionString)
            .AddTestMassTransit()
            .AddAutoMapper(config =>
            {
                config.ConfigureKubernetesAutoMapper();
            })
            .AddKubernetesModule();

        //services.BuildServiceProvider(true).MigrateDatabase();

        return services;
    }

    public static IServiceProvider BuildScopedServiceProvider(this IServiceCollection services)
    {
        return services.BuildServiceProvider(true).CreateScope().ServiceProvider;
    }
}

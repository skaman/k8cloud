using K8Cloud.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace K8Cloud.Shared.Startup;

/// <summary>
/// Startup extensions.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Adds the base database and basic configurations.
    /// </summary>
    /// <param name="services">Services collections.</param>
    /// <param name="optionsAction">Callback actions for options configuration.</param>
    /// <param name="modelBuilderAction">Callback for model builder configuration.</param>
    /// <param name="enabledDetailedLogging">Enabled detailed logging.</param>
    public static void AddK8CloudDatabase(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? optionsAction = null,
        Action<ModelBuilder>? modelBuilderAction = null,
        bool enabledDetailedLogging = false
    )
    {
        K8CloudDbContext.BuildAction = modelBuilderAction;

        services.AddDbContext<K8CloudDbContext>(options =>
        {
            if (enabledDetailedLogging)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.ConfigureWarnings(
                    warningBuilder =>
                        warningBuilder.Log(
                            CoreEventId.FirstWithoutOrderByAndFilterWarning,
                            CoreEventId.RowLimitingOperationWithoutOrderByWarning
                        )
                );
            }

            optionsAction?.Invoke(options);
        });
    }

    /// <summary>
    /// Migrates the database.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public static void MigrateDatabase(this IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<K8CloudDbContext>();
        dbContext.Database.Migrate();
    }

    /// <summary>
    /// Adds the shared services.
    /// </summary>
    /// <param name="services"></param>
    public static void AddSharedServices(this IServiceCollection services)
    {
        //services.AddSingleton<TrackingService>();
    }
}

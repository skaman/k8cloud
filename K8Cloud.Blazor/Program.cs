using K8Cloud.Blazor.Data;
using K8Cloud.Kubernetes.Mappers;
using K8Cloud.Kubernetes.Startup;
using K8Cloud.Shared.Startup;
using MassTransit;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Quartz;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"));
builder.Services.AddQuartz();

// Quartz.Extensions.Hosting hosting
builder.Services.AddQuartzHostedService(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});

builder.Services.AddAutoMapper(config =>
{
    config.ConfigureKubernetesAutoMapper();

    config.AddProfile<ClusterProfile>();
});

// Add the MassTransit services.
builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.AddDelayedMessageScheduler();
    busConfigurator.AddPublishMessageScheduler();

    busConfigurator.AddQuartz();

    busConfigurator.AddInMemoryInboxOutbox();

    busConfigurator.UsingInMemory(
        (context, cfg) =>
        {
            cfg.UseDelayedMessageScheduler();
            cfg.UsePublishMessageScheduler();

            cfg.UseInMemoryOutbox();

            cfg.ConfigureEndpoints(context);
        }
    );

    busConfigurator.ConfigureKubernetesBus();
});

// Add the services.
builder.Services.AddKubernetesModule(builder.Configuration);

// Add the database.
builder.Services.AddK8CloudDatabase(
    optionsAction: options =>
    {
        options.UseSqlite(
            "Data Source=data.db",
            options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
            }
        );
    },
    modelBuilderAction: modelBuilder =>
    {
        modelBuilder.ConfigureKubernetesTables();
    }
);

// Blazor stuff
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddMudServices();

var app = builder.Build();

app.MigrateDatabase();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

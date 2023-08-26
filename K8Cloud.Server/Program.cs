using HotChocolate.Data;
using HotChocolate.Types.Pagination;
using K8Cloud.Kubernetes.Startup;
using K8Cloud.Shared.Database;
using K8Cloud.Shared.GraphQL;
using K8Cloud.Shared.Startup;
using MassTransit;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.
builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"));
builder.Services.AddQuartz();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();

// Quartz.Extensions.Hosting hosting
builder.Services.AddQuartzHostedService(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = true;
});

builder.Services.AddAutoMapper(config =>
{
    config.ConfigureKubernetesAutoMapper();
});

// Add the MassTransit services.
builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();

    var schedulerEndpoint = new Uri("queue:scheduler");
    busConfigurator.AddMessageScheduler(schedulerEndpoint);

    busConfigurator.AddDelayedMessageScheduler();
    busConfigurator.AddPublishMessageScheduler();

    busConfigurator.AddQuartz();
    busConfigurator.AddQuartzConsumers();

    busConfigurator.AddEntityFrameworkOutbox<K8CloudDbContext>(options =>
    {
        options.UsePostgres();
        options.UseBusOutbox();
    });

    busConfigurator.UsingInMemory(
        (context, cfg) =>
        {
            cfg.UseDelayedMessageScheduler();
            cfg.UsePublishMessageScheduler();
            cfg.UseMessageScheduler(schedulerEndpoint);

            cfg.ConfigureEndpoints(context);
        }
    );

    busConfigurator.ConfigureKubernetesBus();
});

// Add the services.
builder.Services.AddSharedServices();
builder.Services.AddKubernetesModule();

// Add the database.
builder.Services.AddK8CloudDatabase(
    optionsAction: options =>
    {
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
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
builder.Services
    .AddGraphQLServer()
    .ModifyOptions(o =>
    {
        o.EnableDefer = true;
        o.EnableStream = true;
    })
    .AddMutationConventions()
    .SetPagingOptions(
        new PagingOptions
        {
            MaxPageSize = 50,
            DefaultPageSize = 10,
            IncludeTotalCount = true
        }
    )
    .RegisterDbContext<K8CloudDbContext>(DbContextKind.Resolver)
    .AddFiltering<ExtendedFilteringConvention>()
    .AddSorting()
    .AddSharedTypes()
    .AddKubernetesTypes();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.Services.MigrateDatabase();
    app.UseWebAssemblyDebugging();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapGraphQL();
app.MapFallbackToFile("index.html");

app.Run();

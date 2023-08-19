using K8Cloud.Web;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Serilog;
using System.Reflection;

Log.Logger = new LoggerConfiguration().WriteTo.BrowserConsole().CreateLogger();

try
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);

    /* this is used instead of .UseSerilog to add Serilog to providers */
    builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    builder.Services.AddScoped(
        sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }
    );
    builder.Services.AddMudServices();

    builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
    builder.Services
        .AddK8CloudClient()
        .ConfigureHttpClient(
            client => client.BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}graphql")
        );

    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An exception occurred while creating the WASM host");
}

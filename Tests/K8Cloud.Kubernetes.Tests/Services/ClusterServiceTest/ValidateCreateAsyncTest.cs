using K8Cloud.Kubernetes.Services;
using K8Cloud.Kubernetes.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace K8Cloud.Kubernetes.Tests.Services.ClusterServiceTest;

public class ValidateCreateAsyncTest : IAsyncLifetime, IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public ValidateCreateAsyncTest(DatabaseFixture fixture)
    {
        _databaseFixture = fixture;
    }

    private IServiceProvider Services { get; set; } = default!;
    private IClusterService ClusterService => Services.GetRequiredService<IClusterService>();

    public Task InitializeAsync()
    {
        Services = new ServiceCollection()
            .ConfigureForKubernetesModule(_databaseFixture.GetConnectionString())
            .AddScoped<IClusterService, ClusterService>()
            .BuildScopedServiceProvider();

        return Task.CompletedTask;
    }

    public Task DisposeAsync() => _databaseFixture.Reset();

    [Fact]
    public async Task T001_should_return_a_valid_result_with_valid_data()
    {
        // act
        var result = await ClusterService.ValidateCreateAsync(Data.ValidClusterData);

        // asserts
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task T002_should_return_an_invalid_result_with_invalid_data()
    {
        // act
        var result = await ClusterService.ValidateCreateAsync(Data.InvalidClusterData);

        // asserts
        Assert.False(result.IsValid);
    }
}

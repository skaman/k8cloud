using AutoMapper;
using K8Cloud.Kubernetes.Entities;
using K8Cloud.Kubernetes.Services;
using K8Cloud.Kubernetes.Tests.Utils;
using K8Cloud.Shared.Database;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace K8Cloud.Kubernetes.Tests.Services.ClusterServiceTest;

public class ValidateUpdateAsyncTest : IAsyncLifetime, IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public ValidateUpdateAsyncTest(DatabaseFixture fixture)
    {
        _databaseFixture = fixture;
    }

    private IServiceProvider Services { get; set; } = default!;
    private IClusterService ClusterService => Services.GetRequiredService<IClusterService>();
    private K8CloudDbContext DbContext => Services.GetRequiredService<K8CloudDbContext>();
    private ClusterEntity OriginalRecord { get; set; } = default!;

    public async Task InitializeAsync()
    {
        Services = new ServiceCollection()
            .ConfigureForKubernetesModule(_databaseFixture.GetConnectionString())
            .BuildScopedServiceProvider();

        var mapper = Services.GetRequiredService<IMapper>();
        OriginalRecord = mapper.Map<ClusterEntity>(Data.ValidClusterData);
        OriginalRecord.Id = NewId.NextGuid();

        await DbContext.Set<ClusterEntity>().AddAsync(OriginalRecord);
        await DbContext.SaveChangesAsync();
    }

    public Task DisposeAsync() => _databaseFixture.Reset();

    [Fact]
    public async Task T001_should_return_a_valid_result_with_valid_data()
    {
        // act
        var result = await ClusterService.ValidateUpdateAsync(
            OriginalRecord.Id,
            Data.ValidClusterData2
        );

        // asserts
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task T002_should_return_an_invalid_result_with_invalid_data()
    {
        // act
        var result = await ClusterService.ValidateUpdateAsync(
            OriginalRecord.Id,
            Data.InvalidClusterData
        );

        // asserts
        Assert.False(result.IsValid);
    }
}

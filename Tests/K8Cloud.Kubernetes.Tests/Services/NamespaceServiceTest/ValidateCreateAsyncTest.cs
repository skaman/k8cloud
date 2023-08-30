using AutoMapper;
using K8Cloud.Cluster.Entities;
using K8Cloud.Cluster.Services;
using K8Cloud.Kubernetes.Tests.Utils;
using K8Cloud.Shared.Database;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace K8Cloud.Kubernetes.Tests.Services.NamespaceServiceTest;

public class ValidateCreateAsyncTest : IAsyncLifetime, IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public ValidateCreateAsyncTest(DatabaseFixture fixture)
    {
        _databaseFixture = fixture;
    }

    private IServiceProvider Services { get; set; } = default!;
    private INamespaceService NamespaceService => Services.GetRequiredService<INamespaceService>();
    private K8CloudDbContext DbContext => Services.GetRequiredService<K8CloudDbContext>();

    private ClusterEntity ClusterRecord { get; set; } = default!;

    public async Task InitializeAsync()
    {
        Services = new ServiceCollection()
            .ConfigureForKubernetesModule(_databaseFixture.GetConnectionString())
            .BuildScopedServiceProvider();

        var mapper = Services.GetRequiredService<IMapper>();

        ClusterRecord = mapper.Map<ClusterEntity>(Data.ValidClusterData);
        ClusterRecord.Id = NewId.NextGuid();

        await DbContext.Set<ClusterEntity>().AddAsync(ClusterRecord);
        await DbContext.SaveChangesAsync();
    }

    public Task DisposeAsync() => _databaseFixture.Reset();

    [Fact]
    public async Task T001_should_return_a_valid_result_with_valid_data()
    {
        // act
        var result = await NamespaceService.ValidateCreateAsync(
            ClusterRecord.Id,
            Data.ValidNamespaceData
        );

        // asserts
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task T002_should_return_an_invalid_result_with_invalid_data()
    {
        // act
        var result = await NamespaceService.ValidateCreateAsync(
            ClusterRecord.Id,
            Data.InvalidNamespaceData
        );

        // asserts
        Assert.False(result.IsValid);
    }
}

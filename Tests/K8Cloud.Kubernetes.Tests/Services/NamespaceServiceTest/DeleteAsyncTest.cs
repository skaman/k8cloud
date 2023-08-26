using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Kubernetes.Entities;
using K8Cloud.Kubernetes.Services;
using K8Cloud.Kubernetes.Tests.Utils;
using K8Cloud.Shared.Database;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace K8Cloud.Kubernetes.Tests.Services.NamespaceServiceTest;

public class DeleteAsyncTest : IAsyncLifetime, IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public DeleteAsyncTest(DatabaseFixture fixture)
    {
        _databaseFixture = fixture;
    }

    private IServiceProvider Services { get; set; } = default!;
    private INamespaceService NamespaceService => Services.GetRequiredService<INamespaceService>();
    private ITestHarness Harness => Services.GetTestHarness();
    private K8CloudDbContext DbContext => Services.GetRequiredService<K8CloudDbContext>();
    private ClusterEntity ClusterRecord { get; set; } = default!;
    private NamespaceEntity OriginalRecord { get; set; } = default!;

    public async Task InitializeAsync()
    {
        Services = new ServiceCollection()
            .ConfigureForKubernetesModule(_databaseFixture.GetConnectionString())
            .AddScoped<INamespaceService, NamespaceService>()
            .BuildScopedServiceProvider();

        var mapper = Services.GetRequiredService<IMapper>();

        ClusterRecord = mapper.Map<ClusterEntity>(Data.ValidClusterData);
        ClusterRecord.Id = NewId.NextGuid();

        var tmpRecord = mapper.Map<NamespaceEntity>(Data.ValidNamespaceData);
        tmpRecord.Id = NewId.NextGuid();
        tmpRecord.ClusterId = ClusterRecord.Id;

        await DbContext.Set<ClusterEntity>().AddAsync(ClusterRecord);
        await DbContext.Set<NamespaceEntity>().AddAsync(tmpRecord);
        await DbContext.SaveChangesAsync();

        // we want to keep an untracked copy of the record
        OriginalRecord = await DbContext
            .Set<NamespaceEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == tmpRecord.Id);
    }

    public Task DisposeAsync() => _databaseFixture.Reset();

    [Fact]
    public async Task T001_should_return_the_deleted_data()
    {
        // act
        var result = await NamespaceService.DeleteAsync(ClusterRecord.Id, OriginalRecord.Id);

        // asserts
        Assert.Equal(OriginalRecord, result);
    }

    [Fact]
    public async Task T002_should_delete_the_record_from_the_database()
    {
        // act
        var result = await NamespaceService.DeleteAsync(ClusterRecord.Id, OriginalRecord.Id);

        // get data
        var exists = await DbContext.Set<NamespaceEntity>().AnyAsync(x => x.Id == result.Id);

        // asserts
        Assert.False(exists);
    }

    [Fact]
    public async Task T010_should_publish_deleted_event()
    {
        // act
        await NamespaceService.DeleteAsync(ClusterRecord.Id, OriginalRecord.Id);

        // asserts
        Assert.True(await Harness.Published.Any<NamespaceDeleted>());
    }

    [Fact]
    public async Task T011_should_published_deleted_event_have_a_valid_timestamp()
    {
        var beforeOperationDateTime = DateTime.UtcNow;

        // act
        await NamespaceService.DeleteAsync(ClusterRecord.Id, OriginalRecord.Id);

        // get data
        var message = (await Harness.Published.SelectAsync<NamespaceDeleted>().First())
            .Context
            .Message;

        // asserts
        Assert.InRange(message.Timestamp, beforeOperationDateTime, DateTime.UtcNow);
    }

    [Fact]
    public async Task T012_should_published_deleted_event_resource_data_match_returned_data()
    {
        // act
        var result = await NamespaceService.DeleteAsync(ClusterRecord.Id, OriginalRecord.Id);

        // get data
        var message = (await Harness.Published.SelectAsync<NamespaceDeleted>().First())
            .Context
            .Message;

        // asserts
        Assert.Equal(result, message.Resource);
    }
}

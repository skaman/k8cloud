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

namespace K8Cloud.Kubernetes.Tests.Services.ClusterServiceTest;

public class DeleteAsyncTest : IAsyncLifetime, IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public DeleteAsyncTest(DatabaseFixture fixture)
    {
        _databaseFixture = fixture;
    }

    private IServiceProvider Services { get; set; } = default!;
    private IClusterService ClusterService => Services.GetRequiredService<IClusterService>();
    private ITestHarness Harness => Services.GetTestHarness();
    private K8CloudDbContext DbContext => Services.GetRequiredService<K8CloudDbContext>();
    private ClusterEntity OriginalRecord { get; set; } = default!;

    public async Task InitializeAsync()
    {
        Services = new ServiceCollection()
            .ConfigureForKubernetesModule(_databaseFixture.GetConnectionString())
            .BuildScopedServiceProvider();

        var mapper = Services.GetRequiredService<IMapper>();
        var tmpRecord = mapper.Map<ClusterEntity>(Data.ValidClusterData);
        tmpRecord.Id = NewId.NextGuid();

        await DbContext.Set<ClusterEntity>().AddAsync(tmpRecord);
        await DbContext.SaveChangesAsync();

        // we want to keep an untracked copy of the record
        OriginalRecord = await DbContext
            .Set<ClusterEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == tmpRecord.Id);
    }

    public Task DisposeAsync() => _databaseFixture.Reset();

    [Fact]
    public async Task T001_should_return_the_deleted_data()
    {
        // act
        var result = await ClusterService.DeleteAsync(OriginalRecord.Id);

        // asserts
        Assert.Equal(OriginalRecord, result);
    }

    [Fact]
    public async Task T002_should_delete_the_record_from_the_database()
    {
        // act
        var result = await ClusterService.DeleteAsync(OriginalRecord.Id);

        // get data
        var exists = await DbContext.Set<ClusterEntity>().AnyAsync(x => x.Id == result.Id);

        // asserts
        Assert.False(exists);
    }

    [Fact]
    public async Task T010_should_publish_deleted_event()
    {
        // act
        await ClusterService.DeleteAsync(OriginalRecord.Id);

        // asserts
        Assert.True(await Harness.Published.Any<ClusterDeleted>());
    }

    [Fact]
    public async Task T011_should_published_deleted_event_have_a_valid_timestamp()
    {
        var beforeOperationDateTime = DateTime.UtcNow;

        // act
        await ClusterService.DeleteAsync(OriginalRecord.Id);

        // get data
        var message = (await Harness.Published.SelectAsync<ClusterDeleted>().First())
            .Context
            .Message;

        // asserts
        Assert.InRange(message.Timestamp, beforeOperationDateTime, DateTime.UtcNow);
    }

    [Fact]
    public async Task T012_should_published_deleted_event_resource_data_match_returned_data()
    {
        // act
        var result = await ClusterService.DeleteAsync(OriginalRecord.Id);

        // get data
        var message = (await Harness.Published.SelectAsync<ClusterDeleted>().First())
            .Context
            .Message;

        // asserts
        Assert.Equal(result, message.Resource);
    }
}

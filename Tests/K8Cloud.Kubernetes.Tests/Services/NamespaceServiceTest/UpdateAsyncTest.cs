using AutoMapper;
using FluentValidation;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Cluster.Entities;
using K8Cloud.Cluster.Services;
using K8Cloud.Kubernetes.Tests.Utils;
using K8Cloud.Shared.Database;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace K8Cloud.Kubernetes.Tests.Services.NamespaceServiceTest;

public class UpdateAsyncTest : IAsyncLifetime, IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public UpdateAsyncTest(DatabaseFixture fixture)
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
    public async Task T001_should_return_a_valid_updated_at()
    {
        var beforeOperationDateTime = DateTime.UtcNow;

        // act
        var result = await NamespaceService.UpdateAsync(
            ClusterRecord.Id,
            OriginalRecord.Id,
            Data.ValidNamespaceData2,
            OriginalRecord.Version.ToString()
        );

        // asserts
        Assert.InRange(result.UpdatedAt, beforeOperationDateTime, DateTime.UtcNow);
    }

    [Fact]
    public async Task T002_should_return_the_old_created_at()
    {
        // wait a couple of milliseconds to ensure the CreatedAt value is different
        await Task.Delay(2);

        // act
        var result = await NamespaceService.UpdateAsync(
            ClusterRecord.Id,
            OriginalRecord.Id,
            Data.ValidNamespaceData2,
            OriginalRecord.Version.ToString()
        );

        // asserts
        Assert.EqualWithMsResolution(OriginalRecord.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task T003_should_return_a_new_version_value()
    {
        // act
        var result = await NamespaceService.UpdateAsync(
            ClusterRecord.Id,
            OriginalRecord.Id,
            Data.ValidNamespaceData2,
            OriginalRecord.Version.ToString()
        );

        // asserts
        Assert.NotEqual((uint)0, result.Version);
        Assert.NotEqual(OriginalRecord.Version, result.Version);
    }

    [Fact]
    public async Task T004_should_return_the_updated_data()
    {
        // act
        var result = await NamespaceService.UpdateAsync(
            ClusterRecord.Id,
            OriginalRecord.Id,
            Data.ValidNamespaceData2,
            OriginalRecord.Version.ToString()
        );

        // asserts
        Assert.Equal(Data.ValidNamespaceData2, result);
    }

    [Fact]
    public async Task T010_should_update_the_record_into_the_database()
    {
        // act
        var result = await NamespaceService.UpdateAsync(
            ClusterRecord.Id,
            OriginalRecord.Id,
            Data.ValidNamespaceData2,
            OriginalRecord.Version.ToString()
        );

        // get data
        var record = await DbContext
            .Set<NamespaceEntity>()
            .SingleOrDefaultAsync(x => x.Id == result.Id);

        // asserts
        Assert.NotNull(record!);
    }

    [Fact]
    public async Task T011_should_not_update_the_record_into_the_database_when_version_exception()
    {
        var wrongVersion = OriginalRecord.Version - 1;

        // act
        try
        {
            await NamespaceService.UpdateAsync(
                ClusterRecord.Id,
                OriginalRecord.Id,
                Data.ValidNamespaceData2,
                wrongVersion.ToString()
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            // ignored
        }

        // get data
        var record = await DbContext
            .Set<NamespaceEntity>()
            .SingleOrDefaultAsync(x => x.Name == Data.ValidNamespaceData2.Name);

        // asserts
        Assert.Null(record!);
    }

    [Fact]
    public async Task T012_should_not_update_the_record_into_the_database_when_validation_exception()
    {
        // act
        try
        {
            await NamespaceService.UpdateAsync(
                ClusterRecord.Id,
                OriginalRecord.Id,
                Data.InvalidNamespaceData,
                OriginalRecord.Version.ToString()
            );
        }
        catch (ValidationException)
        {
            // ignored
        }

        // get data
        var record = await DbContext
            .Set<NamespaceEntity>()
            .SingleOrDefaultAsync(x => x.Name == Data.InvalidNamespaceData.Name);

        // asserts
        Assert.Null(record!);
    }

    [Fact]
    public async Task T013_should_saved_data_match_returned_data()
    {
        // act
        var result = await NamespaceService.UpdateAsync(
            ClusterRecord.Id,
            OriginalRecord.Id,
            Data.ValidNamespaceData2,
            OriginalRecord.Version.ToString()
        );

        // get data
        var record = await DbContext.Set<NamespaceEntity>().SingleAsync(x => x.Id == result.Id);

        // asserts
        Assert.Equal(result, record);
    }

    [Fact]
    public async Task T020_should_emit_exception_when_submitted_invalid_data()
    {
        // act and assert
        await Assert.ThrowsAsync<ValidationException>(
            async () =>
                await NamespaceService.UpdateAsync(
                    ClusterRecord.Id,
                    OriginalRecord.Id,
                    Data.InvalidNamespaceData,
                    OriginalRecord.Version.ToString()
                )
        );
    }

    [Fact]
    public async Task T021_should_emit_exception_when_submitted_wrong_version()
    {
        var wrongVersion = OriginalRecord.Version - 1;

        // act and assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () =>
                await NamespaceService.UpdateAsync(
                    ClusterRecord.Id,
                    OriginalRecord.Id,
                    Data.ValidNamespaceData2,
                    wrongVersion.ToString()
                )
        );
    }

    [Fact]
    public async Task T030_should_publish_updated_event()
    {
        // act
        await NamespaceService.UpdateAsync(
            ClusterRecord.Id,
            OriginalRecord.Id,
            Data.ValidNamespaceData2,
            OriginalRecord.Version.ToString()
        );

        // asserts
        Assert.True(await Harness.Published.Any<NamespaceUpdated>());
    }

    [Fact]
    public async Task T031_should_not_publish_updated_event_when_version_exception()
    {
        var wrongVersion = OriginalRecord.Version - 1;

        // act
        try
        {
            await NamespaceService.UpdateAsync(
                ClusterRecord.Id,
                OriginalRecord.Id,
                Data.ValidNamespaceData2,
                wrongVersion.ToString()
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            // ignored
        }

        // asserts
        Assert.False(await Harness.Published.Any<NamespaceUpdated>());
    }

    [Fact]
    public async Task T032_should_not_publish_updated_event_when_validation_exception()
    {
        // act
        try
        {
            await NamespaceService.UpdateAsync(
                ClusterRecord.Id,
                OriginalRecord.Id,
                Data.InvalidNamespaceData,
                OriginalRecord.Version.ToString()
            );
        }
        catch (ValidationException)
        {
            // ignored
        }

        // asserts
        Assert.False(await Harness.Published.Any<NamespaceUpdated>());
    }

    [Fact]
    public async Task T040_should_published_updated_event_have_a_valid_timestamp()
    {
        var beforeOperationDateTime = DateTime.UtcNow;

        // act
        await NamespaceService.UpdateAsync(
            ClusterRecord.Id,
            OriginalRecord.Id,
            Data.ValidNamespaceData2,
            OriginalRecord.Version.ToString()
        );

        // get data
        var message = (await Harness.Published.SelectAsync<NamespaceUpdated>().First())
            .Context
            .Message;

        // asserts
        Assert.InRange(message.Timestamp, beforeOperationDateTime, DateTime.UtcNow);
    }

    [Fact]
    public async Task T041_should_published_updated_event_resource_data_match_returned_data()
    {
        // act
        var result = await NamespaceService.UpdateAsync(
            ClusterRecord.Id,
            OriginalRecord.Id,
            Data.ValidNamespaceData2,
            OriginalRecord.Version.ToString()
        );

        // get data
        var message = (await Harness.Published.SelectAsync<NamespaceUpdated>().First())
            .Context
            .Message;

        // asserts
        Assert.Equal(result, message.Resource);
    }
}

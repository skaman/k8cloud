using AutoMapper;
using FluentValidation;
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

public class UpdateAsyncTest : IAsyncLifetime, IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public UpdateAsyncTest(DatabaseFixture fixture)
    {
        _databaseFixture = fixture;
    }

    private IServiceProvider _services { get; set; } = default!;
    private IClusterService _clusterService => _services.GetRequiredService<IClusterService>();
    private ITestHarness _harness => _services.GetTestHarness();
    private K8CloudDbContext _dbContext => _services.GetRequiredService<K8CloudDbContext>();
    private ClusterEntity _originalRecord { get; set; } = default!;

    public async Task InitializeAsync()
    {
        _services = new ServiceCollection()
            .ConfigureForKubernetesModule(_databaseFixture.GetConnectionString())
            .AddScoped<IClusterService, ClusterService>()
            .BuildScopedServiceProvider();

        var mapper = _services.GetRequiredService<IMapper>();
        var tmpRecord = mapper.Map<ClusterEntity>(Data.ValidClusterData);
        tmpRecord.Id = NewId.NextGuid();

        await _dbContext.Set<ClusterEntity>().AddAsync(tmpRecord);
        await _dbContext.SaveChangesAsync();

        // we want to keep an untracked copy of the record
        _originalRecord = await _dbContext
            .Set<ClusterEntity>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == tmpRecord.Id);
    }

    public Task DisposeAsync() => _databaseFixture.Reset();

    [Fact]
    public async Task T001_should_return_a_valid_updated_at()
    {
        var beforeOperationDateTime = DateTime.UtcNow;

        // act
        var result = await _clusterService.UpdateAsync(
            _originalRecord.Id,
            Data.ValidClusterData2,
            _originalRecord.Version.ToString()
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
        var result = await _clusterService.UpdateAsync(
            _originalRecord.Id,
            Data.ValidClusterData2,
            _originalRecord.Version.ToString()
        );

        // asserts
        Assert.EqualWithMsResolution(_originalRecord.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task T003_should_return_a_new_version_value()
    {
        // act
        var result = await _clusterService.UpdateAsync(
            _originalRecord.Id,
            Data.ValidClusterData2,
            _originalRecord.Version.ToString()
        );

        // asserts
        Assert.NotEqual((uint)0, result.Version);
        Assert.NotEqual(_originalRecord.Version, result.Version);
    }

    [Fact]
    public async Task T004_should_return_the_updated_data()
    {
        // act
        var result = await _clusterService.UpdateAsync(
            _originalRecord.Id,
            Data.ValidClusterData2,
            _originalRecord.Version.ToString()
        );

        // asserts
        Assert.Equal(Data.ValidClusterData2, result);
    }

    [Fact]
    public async Task T010_should_update_the_record_into_the_database()
    {
        // act
        var result = await _clusterService.UpdateAsync(
            _originalRecord.Id,
            Data.ValidClusterData2,
            _originalRecord.Version.ToString()
        );

        // get data
        var record = await _dbContext
            .Set<ClusterEntity>()
            .SingleOrDefaultAsync(x => x.Id == result.Id);

        // asserts
        Assert.NotNull(record!);
    }

    [Fact]
    public async Task T011_should_not_update_the_record_into_the_database_when_version_exception()
    {
        var wrongVersion = _originalRecord.Version - 1;

        // act
        try
        {
            await _clusterService.UpdateAsync(
                _originalRecord.Id,
                Data.ValidClusterData2,
                wrongVersion.ToString()
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            // ignored
        }

        // get data
        var record = await _dbContext
            .Set<ClusterEntity>()
            .SingleOrDefaultAsync(x => x.ServerName == Data.ValidClusterData2.ServerName);

        // asserts
        Assert.Null(record!);
    }

    [Fact]
    public async Task T012_should_not_update_the_record_into_the_database_when_validation_exception()
    {
        // act
        try
        {
            await _clusterService.UpdateAsync(
                _originalRecord.Id,
                Data.InvalidClusterData,
                _originalRecord.Version.ToString()
            );
        }
        catch (ValidationException)
        {
            // ignored
        }

        // get data
        var record = await _dbContext
            .Set<ClusterEntity>()
            .SingleOrDefaultAsync(x => x.ServerAddress == Data.InvalidClusterData.ServerAddress);

        // asserts
        Assert.Null(record!);
    }

    [Fact]
    public async Task T013_should_saved_data_match_returned_data()
    {
        // act
        var result = await _clusterService.UpdateAsync(
            _originalRecord.Id,
            Data.ValidClusterData2,
            _originalRecord.Version.ToString()
        );

        // get data
        var record = await _dbContext.Set<ClusterEntity>().SingleAsync(x => x.Id == result.Id);

        // asserts
        Assert.Equal(result, record);
    }

    [Fact]
    public async Task T020_should_emit_exception_when_submitted_invalid_data()
    {
        // act and assert
        await Assert.ThrowsAsync<ValidationException>(
            async () =>
                await _clusterService.UpdateAsync(
                    _originalRecord.Id,
                    Data.InvalidClusterData,
                    _originalRecord.Version.ToString()
                )
        );
    }

    [Fact]
    public async Task T021_should_emit_exception_when_submitted_wrong_version()
    {
        var wrongVersion = _originalRecord.Version - 1;

        // act and assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            async () =>
                await _clusterService.UpdateAsync(
                    _originalRecord.Id,
                    Data.ValidClusterData2,
                    wrongVersion.ToString()
                )
        );
    }

    [Fact]
    public async Task T030_should_publish_updated_event()
    {
        // act
        await _clusterService.UpdateAsync(
            _originalRecord.Id,
            Data.ValidClusterData2,
            _originalRecord.Version.ToString()
        );

        // asserts
        Assert.True(await _harness.Published.Any<ClusterUpdated>());
    }

    [Fact]
    public async Task T031_should_not_publish_updated_event_when_version_exception()
    {
        var wrongVersion = _originalRecord.Version - 1;

        // act
        try
        {
            await _clusterService.UpdateAsync(
                _originalRecord.Id,
                Data.ValidClusterData2,
                wrongVersion.ToString()
            );
        }
        catch (DbUpdateConcurrencyException)
        {
            // ignored
        }

        // asserts
        Assert.False(await _harness.Published.Any<ClusterUpdated>());
    }

    [Fact]
    public async Task T032_should_not_publish_updated_event_when_validation_exception()
    {
        // act
        try
        {
            await _clusterService.UpdateAsync(
                _originalRecord.Id,
                Data.InvalidClusterData,
                _originalRecord.Version.ToString()
            );
        }
        catch (ValidationException)
        {
            // ignored
        }

        // asserts
        Assert.False(await _harness.Published.Any<ClusterUpdated>());
    }

    [Fact]
    public async Task T040_should_published_updated_event_have_a_valid_timestamp()
    {
        var beforeOperationDateTime = DateTime.UtcNow;

        // act
        await _clusterService.UpdateAsync(
            _originalRecord.Id,
            Data.ValidClusterData2,
            _originalRecord.Version.ToString()
        );

        // get data
        var message = (await _harness.Published.SelectAsync<ClusterUpdated>().First())
            .Context
            .Message;

        // asserts
        Assert.InRange(message.Timestamp, beforeOperationDateTime, DateTime.UtcNow);
    }

    [Fact]
    public async Task T041_should_published_updated_event_resource_data_match_returned_data()
    {
        // act
        var result = await _clusterService.UpdateAsync(
            _originalRecord.Id,
            Data.ValidClusterData2,
            _originalRecord.Version.ToString()
        );

        // get data
        var message = (await _harness.Published.SelectAsync<ClusterUpdated>().First())
            .Context
            .Message;

        // asserts
        Assert.Equal(result, message.Resource);
    }
}

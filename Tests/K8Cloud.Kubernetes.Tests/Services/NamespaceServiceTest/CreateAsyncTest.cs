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

namespace K8Cloud.Kubernetes.Tests.Services.NamespaceServiceTest;

public class CreateAsyncTest : IAsyncLifetime, IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public CreateAsyncTest(DatabaseFixture fixture)
    {
        _databaseFixture = fixture;
    }

    private IServiceProvider Services { get; set; } = default!;
    private INamespaceService NamespaceService => Services.GetRequiredService<INamespaceService>();
    private ITestHarness Harness => Services.GetTestHarness();
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
    public async Task T001_should_return_a_valid_id()
    {
        // act
        var result = await NamespaceService.CreateAsync(ClusterRecord.Id, Data.ValidNamespaceData);

        // asserts
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task T002_should_return_a_valid_created_and_updated_at()
    {
        var beforeOperationDateTime = DateTime.UtcNow;

        // act
        var result = await NamespaceService.CreateAsync(ClusterRecord.Id, Data.ValidNamespaceData);

        // asserts
        Assert.InRange(result.CreatedAt, beforeOperationDateTime, DateTime.UtcNow);
        Assert.InRange(result.UpdatedAt, beforeOperationDateTime, DateTime.UtcNow);
    }

    [Fact]
    public async Task T003_should_return_a_valid_version()
    {
        // act
        var result = await NamespaceService.CreateAsync(ClusterRecord.Id, Data.ValidNamespaceData);

        // asserts
        Assert.NotEqual((uint)0, result.Version);
    }

    [Fact]
    public async Task T004_should_return_the_inserted_data()
    {
        // act
        var result = await NamespaceService.CreateAsync(ClusterRecord.Id, Data.ValidNamespaceData);

        // asserts
        Assert.Equal(Data.ValidNamespaceData, result);
    }

    [Fact]
    public async Task T010_should_create_the_record_into_the_database()
    {
        // act
        var result = await NamespaceService.CreateAsync(ClusterRecord.Id, Data.ValidNamespaceData);

        // get data
        var record = await DbContext
            .Set<NamespaceEntity>()
            .SingleOrDefaultAsync(x => x.Id == result.Id);

        // asserts
        Assert.NotNull(record!);
    }

    [Fact]
    public async Task T011_should_not_create_the_record_into_the_database_when_validation_exception()
    {
        // act
        try
        {
            await NamespaceService.CreateAsync(ClusterRecord.Id, Data.InvalidNamespaceData);
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
    public async Task T012_should_saved_data_match_returned_data()
    {
        // act
        var result = await NamespaceService.CreateAsync(ClusterRecord.Id, Data.ValidNamespaceData);

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
                await NamespaceService.CreateAsync(ClusterRecord.Id, Data.InvalidNamespaceData)
        );
    }

    [Fact]
    public async Task T030_should_publish_created_event()
    {
        // act
        await NamespaceService.CreateAsync(ClusterRecord.Id, Data.ValidNamespaceData);

        // asserts
        Assert.True(await Harness.Published.Any<NamespaceCreated>());
    }

    [Fact]
    public async Task T031_should_not_publish_created_event_when_validation_exception()
    {
        // act
        try
        {
            await NamespaceService.CreateAsync(ClusterRecord.Id, Data.InvalidNamespaceData);
        }
        catch (ValidationException)
        {
            // ignored
        }

        // asserts
        Assert.False(await Harness.Published.Any<NamespaceCreated>());
    }

    [Fact]
    public async Task T040_should_published_created_event_have_a_valid_timestamp()
    {
        var beforeOperationDateTime = DateTime.UtcNow;

        // act
        await NamespaceService.CreateAsync(ClusterRecord.Id, Data.ValidNamespaceData);

        // get data
        var message = (await Harness.Published.SelectAsync<NamespaceCreated>().First())
            .Context
            .Message;

        // asserts
        Assert.InRange(message.Timestamp, beforeOperationDateTime, DateTime.UtcNow);
    }

    [Fact]
    public async Task T041_should_published_created_event_resource_data_match_returned_data()
    {
        // act
        var result = await NamespaceService.CreateAsync(ClusterRecord.Id, Data.ValidNamespaceData);

        // get data
        var message = (await Harness.Published.SelectAsync<NamespaceCreated>().First())
            .Context
            .Message;

        // asserts
        Assert.Equal(result, message.Resource);
    }
}

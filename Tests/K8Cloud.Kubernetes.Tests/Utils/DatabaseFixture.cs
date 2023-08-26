using K8Cloud.Shared.Startup;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace K8Cloud.Kubernetes.Tests.Utils;

public class DatabaseFixture : IAsyncLifetime
{
    protected readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    private Respawner _respawner = default!;
    private DbConnection _dbConnection = default!;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        new ServiceCollection()
            .AddTestDatabase(_postgreSqlContainer.GetConnectionString())
            .BuildServiceProvider(true)
            .MigrateDatabase();

        _dbConnection = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString());
        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            _dbConnection,
            new RespawnerOptions
            {
                SchemasToInclude = new[] { "public" },
                DbAdapter = DbAdapter.Postgres
            }
        );
    }

    public string GetConnectionString() => _postgreSqlContainer.GetConnectionString();

    public Task Reset()
    {
        return _respawner.ResetAsync(_dbConnection);
    }

    public Task DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }
}

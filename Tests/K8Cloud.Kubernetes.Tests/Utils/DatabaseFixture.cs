using K8Cloud.Shared.Startup;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using System.Data.Common;

namespace K8Cloud.Kubernetes.Tests.Utils;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlDatabase _db = new();

    private Respawner _respawner = default!;
    private DbConnection _dbConnection = default!;

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        new ServiceCollection()
            .AddTestDatabase(_db.GetConnectionString())
            .BuildServiceProvider(true)
            .MigrateDatabase();

        _dbConnection = new NpgsqlConnection(_db.GetConnectionString());
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

    public string GetConnectionString() => _db.GetConnectionString();

    public Task Reset()
    {
        return _respawner.ResetAsync(_dbConnection);
    }

    public Task DisposeAsync()
    {
        return _db.DisposeAsync();
    }
}

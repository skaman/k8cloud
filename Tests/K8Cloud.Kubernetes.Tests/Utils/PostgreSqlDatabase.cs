using Testcontainers.PostgreSql;

namespace K8Cloud.Kubernetes.Tests.Utils;

public class PostgreSqlDatabase
{
    protected readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    public PostgreSqlContainer Container => _postgreSqlContainer;

    public string GetConnectionString() => _postgreSqlContainer.GetConnectionString();

    public Task StartAsync()
    {
        return _postgreSqlContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }
}

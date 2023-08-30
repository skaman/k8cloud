using K8Cloud.Cluster.Services;
using K8Cloud.Kubernetes.Tests.Utils;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace K8Cloud.Kubernetes.Tests.Services.KubernetesServiceTest;

public class CreateNamespaceAsync : IAsyncLifetime, IClassFixture<K3SFixture>
{
    private readonly K3SFixture _k3sFixture;

    public CreateNamespaceAsync(K3SFixture k3sFixture)
    {
        _k3sFixture = k3sFixture;
    }

    private IServiceCollection Services { get; set; } = default!;
    private IKubernetesService KubernetesService { get; set; } = default!;
    private k8s.Kubernetes Client { get; set; } = default!;

    public async Task InitializeAsync()
    {
        Services = new ServiceCollection().ConfigureForKubernetesModule();
        Client = await _k3sFixture.GetClient();

        var kubernetesClientsService = Substitute.For<IKubernetesClientsService>();
        kubernetesClientsService.GetClient(Data.ClusterId).Returns(Client);

        var provider = Services
            .AddScoped(s => kubernetesClientsService)
            .BuildScopedServiceProvider();

        KubernetesService = provider.GetRequiredService<IKubernetesService>();
    }

    public Task DisposeAsync() => _k3sFixture.CleanNamespaces();

    [Fact]
    public async Task T001_should_create_a_namespace_resource_with_valid_data()
    {
        // act
        await KubernetesService.CreateNamespaceAsync(Data.ValidNamespaceResource);

        // get value
        var k3sValue = await Client.CoreV1.ReadNamespaceAsync(Data.ValidNamespaceResource.Name);

        // assert
        Assert.Equal(k3sValue.Metadata.Name, Data.ValidNamespaceData.Name);
    }

    [Fact]
    public async Task T002_should_emit_kubernetes_exception_with_invalid_data()
    {
        // act and assert
        await Assert.ThrowsAsync<Cluster.Exceptions.KubernetesException>(
            async () => await KubernetesService.CreateNamespaceAsync(Data.InvalidNamespaceResource)
        );
    }
}

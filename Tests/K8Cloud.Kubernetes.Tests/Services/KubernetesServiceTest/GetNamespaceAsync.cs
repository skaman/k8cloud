using AutoMapper;
using K8Cloud.Cluster.Services;
using K8Cloud.Kubernetes.Tests.Utils;
using k8s;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace K8Cloud.Kubernetes.Tests.Services.KubernetesServiceTest;

public class GetNamespaceAsync : IAsyncLifetime, IClassFixture<K3SFixture>
{
    private readonly K3SFixture _k3sFixture;

    public GetNamespaceAsync(K3SFixture k3sFixture)
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

        var mapper = provider.GetRequiredService<IMapper>();
        await Client.CoreV1.CreateNamespaceAsync(
            mapper.Map<V1Namespace>(Data.ValidNamespaceResource)
        );
    }

    public Task DisposeAsync() => _k3sFixture.CleanNamespaces();

    [Fact]
    public async Task T001_should_return_the_namespace_resource_if_exists()
    {
        // act
        var result = await KubernetesService.GetNamespaceAsync(
            Data.ValidNamespaceResource.ClusterId,
            Data.ValidNamespaceResource.Name
        );

        // assert
        Assert.Equal(Data.ValidNamespaceResource, result);
    }

    [Fact]
    public async Task T002_should_emit_kubernetes_exception_if_not_exists()
    {
        // act and assert
        await Assert.ThrowsAsync<Cluster.Exceptions.KubernetesException>(
            async () =>
                await KubernetesService.GetNamespaceAsync(
                    Data.ValidNamespaceResource.ClusterId,
                    "notexists"
                )
        );
    }
}

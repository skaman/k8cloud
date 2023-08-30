using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Cluster.Services;
using K8Cloud.Kubernetes.Tests.Utils;
using k8s;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace K8Cloud.Kubernetes.Tests.Services.KubernetesServiceTest;

public class UpdateNamespaceAsync : IAsyncLifetime, IClassFixture<K3SFixture>
{
    private readonly K3SFixture _k3sFixture;

    public UpdateNamespaceAsync(K3SFixture k3sFixture)
    {
        _k3sFixture = k3sFixture;
    }

    private IServiceCollection Services { get; set; } = default!;
    private IKubernetesService KubernetesService { get; set; } = default!;
    private IMapper Mapper { get; set; } = default!;
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
        Mapper = provider.GetRequiredService<IMapper>();
    }

    public Task DisposeAsync() => _k3sFixture.CleanNamespaces();

    [Fact]
    public async Task T001_should_update_a_namespace_resource_with_valid_data()
    {
        // prepare
        //await Client.CoreV1.CreateNamespaceAsync(
        //    Mapper.Map<V1Namespace>(Data.ValidNamespaceResource)
        //);

        // act
        await KubernetesService.UpdateNamespaceAsync(Data.ValidNamespaceResource);
        await KubernetesService.UpdateNamespaceAsync(Data.ValidNamespaceResource2);

        // get value
        var k3sValue = await Client.CoreV1.ReadNamespaceAsync(Data.ValidNamespaceResource.Name);

        // assert
        Assert.Equal(Data.ValidNamespaceResource2.Version, Mapper.Map<NamespaceResource>(k3sValue).Version);
    }

    [Fact]
    public async Task T002_should_emit_kubernetes_exception_with_invalid_data()
    {
        // prepare
        await Client.CoreV1.CreateNamespaceAsync(
            Mapper.Map<V1Namespace>(Data.ValidNamespaceResource)
        );

        // act and assert
        await Assert.ThrowsAsync<Cluster.Exceptions.KubernetesException>(
            async () => await KubernetesService.UpdateNamespaceAsync(Data.InvalidNamespaceResource)
        );
    }

    [Fact]
    public async Task T003_should_emit_kubernetes_exception_if_not_exists()
    {
        // act and assert
        await Assert.ThrowsAsync<Cluster.Exceptions.KubernetesException>(
            async () =>
                await KubernetesService.UpdateNamespaceAsync(Data.ValidNamespaceResource2)
        );
    }
}

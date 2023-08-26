using k8s;
using k8s.KubeConfigModels;
using Testcontainers.K3s;

namespace K8Cloud.Kubernetes.Tests.Utils;

public class K3SFixture : IAsyncLifetime
{
    private readonly K3sContainer _k3sContainer = new K3sBuilder().Build();

    public Task InitializeAsync()
    {
        return _k3sContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _k3sContainer.DisposeAsync().AsTask();
    }

    public async Task<K8SConfiguration> GetConfiguration()
    {
        return KubernetesYaml.Deserialize<K8SConfiguration>(
            await _k3sContainer.GetKubeconfigAsync()
        );
    }
}

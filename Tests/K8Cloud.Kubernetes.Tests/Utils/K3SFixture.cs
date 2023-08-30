using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using Testcontainers.K3s;

namespace K8Cloud.Kubernetes.Tests.Utils;

public class K3SFixture : IAsyncLifetime
{
    private static string[] NamespacesKeepList = new string[]
    {
        "default",
        "kube-system",
        "kube-public",
        "kube-node-lease",
    };

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

    public async Task<k8s.Kubernetes> GetClient()
    {
        var config = KubernetesClientConfiguration.BuildConfigFromConfigObject(
            await GetConfiguration()
        );
        return new k8s.Kubernetes(config);
    }

    public async Task CleanNamespaces()
    {
        var client = await GetClient();
        var response = await client.CoreV1.ListNamespaceAsync();
        foreach (var item in response.Items)
        {
            if (NamespacesKeepList.Contains(item.Metadata.Name))
            {
                continue;
            }
            await client.CoreV1.DeleteNamespaceAsync(item.Metadata.Name, gracePeriodSeconds: 0);
            bool isDeleted = false;
            while (!isDeleted)
            {
                try
                {
                    await client.CoreV1.ReadNamespaceAsync(item.Metadata.Name);
                }
                catch (Exception)
                {
                    isDeleted = true;
                }
            }
        }
    }
}

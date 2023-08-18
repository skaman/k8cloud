using K8Cloud.Kubernetes.Database;
using K8Cloud.Shared.Database;
using k8s.KubeConfigModels;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace K8Cloud.Kubernetes.Services;

internal class KubernetesClientsService
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<Guid, k8s.Kubernetes> _clients = new();

    public KubernetesClientsService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public k8s.Kubernetes GetClient(Guid clusterId)
    {
        return _clients.GetOrAdd(clusterId, CreateClient); // TODO: remove unused clients after a while
    }

    private k8s.Kubernetes CreateClient(Guid guid)
    {
        var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<K8CloudDbContext>();
        var cluster = dbContext.ClustersReadOnly().Single(x => x.Id == guid);
        var config = KubernetesClientConfiguration.BuildConfigFromConfigObject(
            new K8SConfiguration
            {
                Clusters = new List<Cluster>
                {
                    new Cluster
                    {
                        Name = cluster.ServerName,
                        ClusterEndpoint = new ClusterEndpoint
                        {
                            Server = cluster.ServerAddress,
                            CertificateAuthorityData = cluster.ServerCertificateAuthorityData
                        }
                    }
                },
                Users = new List<User>
                {
                    new User
                    {
                        Name = cluster.UserName,
                        UserCredentials = new UserCredentials
                        {
                            ClientCertificateData = cluster.UserCredentialsCertificateData,
                            ClientKeyData = cluster.UserCredentialsKeyData
                        }
                    }
                },
                Contexts = new List<Context>
                {
                    new Context
                    {
                        Name = cluster.ServerName,
                        ContextDetails = new ContextDetails
                        {
                            Cluster = cluster.ServerName,
                            Namespace = cluster.Namespace,
                            User = cluster.UserName
                        }
                    }
                },
                CurrentContext = cluster.ServerName
            }
        );
        return new k8s.Kubernetes(config);
    }
}

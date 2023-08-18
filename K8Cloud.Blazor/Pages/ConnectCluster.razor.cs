using FluentValidation;
using K8Cloud.Blazor.Utils;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.RequestResponse;
using k8s;
using k8s.KubeConfigModels;
using MassTransit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace K8Cloud.Blazor.Pages;

public partial class ConnectCluster
{
    public record ConnectClusterData
    {
        public string ServerName { get; set; } = null!;
        public string ServerAddress { get; set; } = null!;
        public string ServerCertificateAuthorityData { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string UserCredentialsCertificateData { get; set; } = null!;
        public string UserCredentialsKeyData { get; set; } = null!;
        public string Namespace { get; set; } = null!;
    }

    public class ConnectClusterDataValidator : MudAbstractValidator<ConnectClusterData>
    {
        public ConnectClusterDataValidator()
        {
            RuleFor(x => x.ServerName).NotEmpty();
            RuleFor(x => x.ServerAddress).NotEmpty();
            RuleFor(x => x.ServerCertificateAuthorityData).NotEmpty();
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.UserCredentialsCertificateData).NotEmpty();
            RuleFor(x => x.UserCredentialsKeyData).NotEmpty();
            RuleFor(x => x.Namespace).NotEmpty();
        }
    }

    [Inject]
    private IPublishEndpoint PublishEndpoint { get; set; } = null!;

    [Inject]
    private ILogger<ConnectCluster> Logger { get; set; } = null!;

    private MudForm Form { get; set; } = null!;

    private ConnectClusterDataValidator Validator { get; } = new();

    private ConnectClusterData Data { get; } = new();

    private async Task Submit()
    {
        await Form.Validate();

        if (Form.IsValid)
        {
            await PublishEndpoint.Publish(
                new AddCluster
                {
                    Id = NewId.NextGuid(),
                    Data = new ClusterData
                    {
                        ServerName = Data.ServerName,
                        ServerAddress = Data.ServerAddress,
                        ServerCertificateAuthorityData = Data.ServerCertificateAuthorityData,
                        UserName = Data.UserName,
                        UserCredentialsCertificateData = Data.UserCredentialsCertificateData,
                        UserCredentialsKeyData = Data.UserCredentialsKeyData,
                        Namespace = Data.Namespace
                    }
                }
            );
        }
    }

    private async Task LoadConfigurationFile(IBrowserFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var config = await KubernetesYaml.LoadFromStreamAsync<K8SConfiguration>(stream);

            var currentContext = config.CurrentContext;
            var context = config.Contexts.First(x => x.Name == currentContext);
            var cluster = config.Clusters.First(x => x.Name == context.ContextDetails.Cluster);
            var user = config.Users.First(x => x.Name == context.ContextDetails.User);

            Data.ServerName = context.ContextDetails.Cluster;
            Data.ServerAddress = cluster.ClusterEndpoint.Server;
            Data.ServerCertificateAuthorityData = cluster.ClusterEndpoint.CertificateAuthorityData;
            Data.UserName = context.ContextDetails.User;
            Data.UserCredentialsCertificateData = user.UserCredentials.ClientCertificateData;
            Data.UserCredentialsKeyData = user.UserCredentials.ClientKeyData;
            Data.Namespace = context.ContextDetails.Namespace;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load configuration file");

            Snackbar.Add("Failed to load configuration file", MudBlazor.Severity.Error);
        }
    }
}

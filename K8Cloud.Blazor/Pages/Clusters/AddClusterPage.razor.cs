using AutoMapper;
using K8Cloud.Blazor.Components.Clusters;
using K8Cloud.Blazor.Extensions;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.RequestResponse;
using k8s;
using k8s.KubeConfigModels;
using MassTransit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;

namespace K8Cloud.Blazor.Pages.Clusters;

public partial class AddClusterPage
{
    [Inject]
    private ILogger<AddCluster> Logger { get; set; } = null!;

    [Inject]
    private IRequestClient<ValidateProperty<AddCluster>> ValidatePropertyClient { get; set; } =
        null!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    [Inject]
    private IMapper Mapper { get; set; } = null!;

    private List<BreadcrumbItem>? BreadcrumbItems { get; set; } =
        new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Clusters", href: "/clusters"),
            new BreadcrumbItem($"Add", href: null, disabled: true)
        };

    private ClusterForm.ClusterFormData Data { get; set; } = new();

    private MudForm Form { get; set; } = null!;

    private bool IsLoading { get; set; }

    protected override async Task OnInitializedAsync() { }

    private async Task OnBeforeInternalNavigation(LocationChangingContext context)
    {
        if (Form.IsTouched)
            await context.AskToLeavePage(JSRuntime);
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue =>
        async (model, propertyName) =>
        {
            var formData = (ClusterForm.ClusterFormData)model;
            var result = await ValidatePropertyClient.GetResponse<ValidatePropertyResponse>(
                new ValidateProperty<AddCluster>
                {
                    Data = new AddCluster
                    {
                        Id = Guid.Empty,
                        Data = Mapper.Map<ClusterData>(formData)
                    },
                    PropertyName = $"Data.{propertyName}"
                }
            );
            if (result.Message.IsValid)
                return Array.Empty<string>();
            return result.Message.Errors.Select(e => e.Message);
        };

    private void Save() { }

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

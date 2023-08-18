using K8Cloud.Blazor.Components.Clusters;
using K8Cloud.Blazor.Extensions;
using k8s.KubeConfigModels;
using k8s;
using MassTransit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using K8Cloud.Contracts.Kubernetes.RequestResponse;

namespace K8Cloud.Blazor.Pages.Clusters;

public partial class EditClusterPage
{
    [Inject]
    private ILogger<EditCluster> Logger { get; set; } = null!;

    [Inject]
    private IRequestClient<GetClusterData> GetClusterDataClient { get; set; } = null!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    private string ClusterName { get; set; } = string.Empty;

    private List<BreadcrumbItem>? BreadcrumbItems { get; set; }

    private ClusterForm.ClusterFormData Data { get; set; } = new();

    private bool IsFormTouched { get; set; }

    private bool IsLoading { get; set; }

    private void UpdateBreadcrumb()
    {
        BreadcrumbItems = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Clusters", href: "/clusters"),
            new BreadcrumbItem(ClusterName, href: $"/clusters/{Id}"),
            new BreadcrumbItem($"Edit", href: null, disabled: true)
        };
    }

    protected override async Task OnInitializedAsync()
    {
        UpdateBreadcrumb();
        IsLoading = true;
        var response = await GetClusterDataClient.GetResponse<GetClusterDataResponse>(
            new GetClusterSummary { ClusterId = Id }
        );
        ClusterName = response.Message.Data.ServerName;
        UpdateBreadcrumb();

        Data = new ClusterForm.ClusterFormData
        {
            ServerName = response.Message.Data.ServerName,
            ServerAddress = response.Message.Data.ServerAddress,
            ServerCertificateAuthorityData = response.Message.Data.ServerCertificateAuthorityData,
            UserName = response.Message.Data.UserName,
            UserCredentialsCertificateData = response.Message.Data.UserCredentialsCertificateData,
            UserCredentialsKeyData = response.Message.Data.UserCredentialsKeyData,
            Namespace = response.Message.Data.Namespace
        };
        IsLoading = false;
    }

    private async Task OnBeforeInternalNavigation(LocationChangingContext context)
    {
        if (IsFormTouched)
            await context.AskToLeavePage(JSRuntime);
    }

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

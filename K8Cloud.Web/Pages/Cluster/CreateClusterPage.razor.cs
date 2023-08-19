using AutoMapper;
using K8Cloud.Web.Components;
using K8Cloud.Web.Components.Clusters;
using K8Cloud.Web.Extensions;
using k8s;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using StrawberryShake;

namespace K8Cloud.Web.Pages.Cluster;

public partial class CreateClusterPage
{
    [Inject]
    private K8CloudClient Client { get; set; } = default!;

    [Inject]
    private ILogger<CreateClusterPage> Logger { get; set; } = default!;

    [Inject]
    private IMapper Mapper { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private List<BreadcrumbItem>? BreadcrumbItems { get; set; } =
        new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Clusters", href: "/clusters"),
            new BreadcrumbItem($"Add", href: null, disabled: true)
        };

    private ClusterForm.ClusterFormData Data { get; set; } = new();
    private EditContext EditContext { get; set; } = default!;
    private ValidationContext ValidationContext { get; set; } = default!;
    private ModifiedContext ModifiedContext { get; set; } = default!;

    private bool IsSaving { get; set; }
    private bool IsCancelDisabled => IsSaving;
    private bool IsSaveDisabled => IsSaving || !ModifiedContext.IsModified();

    protected override void OnInitialized()
    {
        EditContext = new EditContext(Data);
        ValidationContext = new ValidationContext(
            EditContext,
            Logger,
            Snackbar,
            async (model, cancellationToken) =>
            {
                var result = await Client.ValidateCreateCluster
                    .ExecuteAsync(Mapper.Map<ClusterDataInput>(model), cancellationToken)
                    .ConfigureAwait(false);
                return result.Data?.ValidateCreateCluster.ValidationResult;
            }
        );
        ModifiedContext = new ModifiedContext(EditContext);
        ModifiedContext.OnModifiedChanged += (sender, args) =>
        {
            StateHasChanged();
        };
    }

    private async Task OnBeforeInternalNavigation(LocationChangingContext context)
    {
        if (ModifiedContext.IsModified())
            await context.AskToLeavePage(JSRuntime);
    }

    private async Task Save()
    {
        IsSaving = true;
        var result = await Client.CreateCluster.ExecuteAsync(Mapper.Map<ClusterDataInput>(Data));
        if (result.IsSuccessResult())
        {
            if (result.Data!.CreateCluster.Errors != null)
            {
                ValidationContext.UpdateValidation(result.Data!.CreateCluster.Errors);
            }
            else
            {
                ModifiedContext.MarkAsUnmodified();
                NavigationManager.NavigateTo(
                    $"/clusters/{result.Data!.CreateCluster.ClusterRecord!.Id}"
                );
            }
        }
        else
        {
            Snackbar.AddClientErrors(result.Errors);
        }
        IsSaving = false;
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

            ModifiedContext.Update();
            await ValidationContext.Validate();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load configuration file");

            Snackbar.Add("Failed to load configuration file", Severity.Error);
        }
    }
}

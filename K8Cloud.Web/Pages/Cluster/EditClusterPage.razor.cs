using AutoMapper;
using K8Cloud.Web.Components;
using K8Cloud.Web.Components.Clusters;
using K8Cloud.Web.Extensions;
using k8s.KubeConfigModels;
using k8s;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;
using StrawberryShake;

namespace K8Cloud.Web.Pages.Cluster;

public partial class EditClusterPage
{
    private readonly ClusterForm.ClusterFormData _data = new();
    private List<BreadcrumbItem>? _breadcrumbItems;
    private string? _version;

    [Inject]
    private K8CloudClient Client { get; set; } = default!;

    [Inject]
    private ILogger<EditClusterPage> Logger { get; set; } = default!;

    [Inject]
    private IMapper Mapper { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    private EditContext EditContext { get; set; } = default!;
    private ValidationContext ValidationContext { get; set; } = default!;
    private ModifiedContext ModifiedContext { get; set; } = default!;

    private bool IsSaving { get; set; }
    private bool IsCancelDisabled => IsSaving;
    private bool IsSaveDisabled => IsSaving || !ModifiedContext.IsModified();

    private List<BreadcrumbItem> GetBreadcrumbItems(IEditClusterQuery_ClusterById queryResult)
    {
        if (_breadcrumbItems == null)
        {
            _breadcrumbItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Clusters", href: "/clusters"),
                new BreadcrumbItem(queryResult.ServerName, href: $"/clusters/{Id}"),
                new BreadcrumbItem($"Edit", href: null, disabled: true)
            };
        }
        return _breadcrumbItems;
    }

    private ClusterForm.ClusterFormData UpdateAndGetData(IEditClusterQuery_ClusterById queryResult)
    {
        if (_version != queryResult.Version)
        {
            Mapper.Map(queryResult, _data);
            _version = queryResult.Version;

            ModifiedContext.MarkAsUnmodified();
        }
        return _data;
    }

    protected override void OnInitialized()
    {
        EditContext = new EditContext(_data);
        ValidationContext = new ValidationContext(
            EditContext,
            Logger,
            Snackbar,
            async (model, cancellationToken) =>
            {
                var result = await Client.ValidateEditCluster
                    .ExecuteAsync(Id, Mapper.Map<ClusterDataInput>(model), cancellationToken)
                    .ConfigureAwait(false);
                return result.Data?.ValidateUpdateCluster.ValidationResult;
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
        var result = await Client.EditCluster.ExecuteAsync(
            Id,
            Mapper.Map<ClusterDataInput>(_data),
            _version!
        );
        if (result.IsSuccessResult())
        {
            if (result.Data!.UpdateCluster.Errors != null)
            {
                ValidationContext.UpdateValidation(result.Data!.UpdateCluster.Errors);
            }
            else
            {
                ModifiedContext.MarkAsUnmodified();
                Snackbar.Add($"Cluster '{_data.ServerName}' updated", Severity.Success);
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

            _data.ServerName = context.ContextDetails.Cluster;
            _data.ServerAddress = cluster.ClusterEndpoint.Server;
            _data.ServerCertificateAuthorityData = cluster.ClusterEndpoint.CertificateAuthorityData;
            _data.UserName = context.ContextDetails.User;
            _data.UserCredentialsCertificateData = user.UserCredentials.ClientCertificateData;
            _data.UserCredentialsKeyData = user.UserCredentials.ClientKeyData;
            _data.Namespace = context.ContextDetails.Namespace;

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

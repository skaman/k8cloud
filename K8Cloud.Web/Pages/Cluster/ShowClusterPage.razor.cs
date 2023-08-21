using K8Cloud.Web.Extensions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using StrawberryShake;

namespace K8Cloud.Web.Pages.Cluster;

public partial class ShowClusterPage
{
    private List<BreadcrumbItem>? _breadcrumbItems;

    [Inject]
    private K8CloudClient Client { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter]
    public Guid Id { get; set; }

    private List<BreadcrumbItem>? BreadcrumbItems { get; set; }

    private bool IsLoading { get; set; }

    private List<BreadcrumbItem> GetBreadcrumbItems(IShowClusterQuery_ClusterById queryResult)
    {
        if (_breadcrumbItems == null)
        {
            _breadcrumbItems = new List<BreadcrumbItem>
            {
                new BreadcrumbItem(
                    "Clusters",
                    href: "/clusters",
                    icon: Icons.Material.TwoTone.Home
                ),
                new BreadcrumbItem(queryResult.ServerName, href: $"/clusters/{Id}")
            };
        }
        return _breadcrumbItems;
    }

    private string? GetConditionToolTip(IShowClusterQuery_ClusterById_Status_Nodes node, string type)
    {
        return node.Conditions.FirstOrDefault(x => x.Type == type)?.Message;
    }
    
    private string GetReadyValue(IShowClusterQuery_ClusterById_Status_Nodes node)
    {
        var isOperative = node.Conditions.Any(x => x.Type == "Ready" && x.IsOperative);
        return isOperative ? "Ready" : "Not Ready";
    }
    
    private Color GetConditionColor(IShowClusterQuery_ClusterById_Status_Nodes node, string type)
    {
        var isOperative = node.Conditions.Any(x => x.Type == type && x.IsOperative);
        return isOperative ? Color.Success : Color.Error;
    }

    private Color GetStatusColor(bool? isOperative)
    {
        return isOperative switch
        {
            true => Color.Success,
            false => Color.Error,
            _ => Color.Default
        };
    }

    private string GetStatusLabel(bool? isOperative)
    {
        return isOperative switch
        {
            true => "Operative",
            false => "Not operative",
            _ => "Unkwnown"
        };
    }

    private async Task Delete(IShowClusterQuery_ClusterById queryResult)
    {
        var result = await DialogService.ShowSafeDeleteDialog(
            $"Delete cluster {queryResult.ServerName}",
            queryResult.ServerName
        );

        if (!result.Canceled && (bool)result.Data)
        {
            var response = await Client.DeleteCluster.ExecuteAsync(Id);
            if (response.IsSuccessResult())
            {
                Snackbar.Add("Server Deleted", Severity.Success);
                NavigationManager.NavigateTo("/clusters");
            }
            else
            {
                Snackbar.AddClientErrors(response.Errors);
            }
        }
    }
}

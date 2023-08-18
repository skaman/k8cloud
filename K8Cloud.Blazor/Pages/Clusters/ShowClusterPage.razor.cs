using K8Cloud.Blazor.Extensions;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.RequestResponse;
using MassTransit;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace K8Cloud.Blazor.Pages.Clusters;

public partial class ShowClusterPage
{
    [Inject]
    private IRequestClient<ListNodes> ListNodesClient { get; set; } = null!;

    [Inject]
    private IRequestClient<GetClusterSummary> GetClusterSummaryClient { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    private string ClusterName { get; set; } = string.Empty;

    private NodeInfo[] Nodes { get; set; } = Array.Empty<NodeInfo>();

    private List<BreadcrumbItem>? BreadcrumbItems { get; set; }

    private bool IsLoading { get; set; }

    private void UpdateBreadcrumb()
    {
        BreadcrumbItems = new List<BreadcrumbItem>
        {
            new BreadcrumbItem("Clusters", href: "/clusters"),
            new BreadcrumbItem(ClusterName, href: null, disabled: true)
        };
    }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        UpdateBreadcrumb();
        var summary = await GetClusterSummaryClient.GetResponse<GetClusterSummaryResponse>(
            new GetClusterSummary { ClusterId = Id }
        );
        ClusterName = summary.Message.Data.Name;
        UpdateBreadcrumb();

        var response = await ListNodesClient.GetResponse<ListNodesResponse>(
            new ListNodes { ClusterId = Id }
        );
        Nodes = response.Message.Nodes;
        IsLoading = false;
    }

    private string? GetConditionToolTip(NodeInfo node, string type)
    {
        return Array.Find(node.Conditions, x => x.Type == type)?.Message;
    }

    private string GetReadyValue(NodeInfo node)
    {
        var isOperative = Array.Find(node.Conditions, x => x.Type == "Ready")?.IsOperative ?? false;
        return isOperative ? "Ready" : "Not Ready";
    }

    private Color GetConditionColor(NodeInfo node, string type)
    {
        var isOperative = Array.Find(node.Conditions, x => x.Type == type)?.IsOperative ?? false;
        return isOperative ? Color.Success : Color.Error;
    }

    private async Task Delete()
    {
        var result = await DialogService.ShowSafeDeleteDialog(
            $"Delete cluster {ClusterName}",
            ClusterName
        );
        if (!result.Canceled && (bool)result.Data)
        {
            Snackbar.Add("Server Deleted", Severity.Success);
        }
    }
}

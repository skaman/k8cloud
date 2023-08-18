using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.RequestResponse;
using MassTransit;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace K8Cloud.Blazor.Pages.Clusters;

public partial class ClustersPage
{
    [Inject]
    private IRequestClient<ListClusterSummaries> ListClusterSummariesClient { get; set; } = null!;

    private ClusterSummary[] Items { get; set; } = Array.Empty<ClusterSummary>();

    private bool IsLoading { get; set; }

    private List<BreadcrumbItem> BreadcrumbItems { get; } =
        new List<BreadcrumbItem> { new BreadcrumbItem("Clusters", href: null, disabled: true) };

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        var response = await ListClusterSummariesClient.GetResponse<ListClusterSummariesResponse>(
            new ListClusterSummaries()
        );
        Items = response.Message.Items;
        IsLoading = false;
    }
}

using K8Cloud.Web.Components;
using K8Cloud.Web.Components.Contexts;
using MudBlazor;

namespace K8Cloud.Web.Pages.Cluster;

public partial class ClustersPage
{
    private List<BreadcrumbItem> BreadcrumbItems { get; } =
        new List<BreadcrumbItem>
        {
            new BreadcrumbItem(
                "Clusters",
                href: null,
                disabled: true,
                icon: Icons.Material.TwoTone.Home
            )
        };

    private SortingContext SortingContext { get; } = new SortingContext();

    private int PageSize { get; set; } = 10;
    private int CurrentPage { get; set; } = 1;
    private int Skip => (CurrentPage - 1) * PageSize;
    private string? Search { get; set; }

    private SortingContext.Item SortServerName { get; }
    private SortingContext.Item SortServerAddress { get; }

    public ClustersPage()
    {
        SortServerName = new SortingContext.Item(SortingContext, SortDirection.Ascending);
        SortServerAddress = new SortingContext.Item(SortingContext);
    }

    private IReadOnlyList<ClusterResourceSortInput> GetSortInput()
    {
        var sortInput = new ClusterResourceSortInput();
        SortServerName.SetToGraphQL(value => sortInput.ServerName = value);
        SortServerAddress.SetToGraphQL(value => sortInput.ServerAddress = value);

        return new List<ClusterResourceSortInput> { sortInput };
    }

    private ClusterResourceFilterInput? GetFilterInput()
    {
        if (string.IsNullOrEmpty(Search))
        {
            return null;
        }

        return new ClusterResourceFilterInput
        {
            Or = new List<ClusterResourceFilterInput>
            {
                new ClusterResourceFilterInput
                {
                    ServerName = new StringOperationFilterInput { ContainsInvariant = Search }
                },
                new ClusterResourceFilterInput
                {
                    ServerAddress = new StringOperationFilterInput { ContainsInvariant = Search }
                }
            }
        };
    }

    void OnSearch(string debouncedText)
    {
        Search = debouncedText;
        StateHasChanged();
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
}

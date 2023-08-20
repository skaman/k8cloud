using K8Cloud.Web.Components.Contexts;
using MudBlazor;

namespace K8Cloud.Web.Pages.Cluster;

public partial class ClustersPage
{
    private List<BreadcrumbItem> BreadcrumbItems { get; } =
        new List<BreadcrumbItem> { new BreadcrumbItem("Clusters", href: null, disabled: true) };

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

    private int GetPageCount(int totalCount) => (int)Math.Ceiling(totalCount / (double)PageSize);

    private IReadOnlyList<ClusterRecordSortInput> GetSortInput()
    {
        var sortInput = new ClusterRecordSortInput();
        SortServerName.SetToGraphQL(value => sortInput.ServerName = value);
        SortServerAddress.SetToGraphQL(value => sortInput.ServerAddress = value);

        return new List<ClusterRecordSortInput> { sortInput };
    }

    private ClusterRecordFilterInput? GetFilterInput()
    {
        if (string.IsNullOrEmpty(Search))
        {
            return null;
        }

        return new ClusterRecordFilterInput
        {
            Or = new List<ClusterRecordFilterInput>
            {
                new ClusterRecordFilterInput
                {
                    ServerName = new StringOperationFilterInput { ContainsInvariant = Search }
                },
                new ClusterRecordFilterInput
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
}

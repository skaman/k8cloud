using MudBlazor;

namespace K8Cloud.Web.Pages.Cluster;

public partial class ClustersPage
{
    private List<BreadcrumbItem> BreadcrumbItems { get; } =
        new List<BreadcrumbItem> { new BreadcrumbItem("Clusters", href: null, disabled: true) };
}

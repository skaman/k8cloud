using K8Cloud.Blazor.Dialogs;
using MudBlazor;

namespace K8Cloud.Blazor.Extensions;

public static class DialogServiceExtensions
{
    public static async Task<DialogResult> ShowSafeDeleteDialog(
        this IDialogService dialogService,
        string title,
        string resourceName
    )
    {
        var parameters = new DialogParameters<SafeDeleteDialog>
        {
            { x => x.Title, title },
            { x => x.ResourceName, resourceName }
        };
        var options = new DialogOptions { ClassBackground = "blurry-background" };
        var dialog = await dialogService.ShowAsync<SafeDeleteDialog>(title, parameters, options);
        return await dialog.Result;
    }
}

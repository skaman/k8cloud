using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace K8Cloud.Blazor.Dialogs;

public partial class SafeDeleteDialog
{
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public string Title { get; set; } = null!;

    [Parameter]
    public string ResourceName { get; set; } = null!;
    public string InsertedResourceName { get; set; } = null!;
    public bool IsValidResourceName { get; set; }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private void Confirm()
    {
        MudDialog.Close(DialogResult.Ok(IsValidResourceName));
    }

    private void InsertedResourceNameChanged(string value)
    {
        IsValidResourceName = value == ResourceName;
    }
}

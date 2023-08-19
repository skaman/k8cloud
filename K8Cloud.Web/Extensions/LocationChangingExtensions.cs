using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace K8Cloud.Web.Extensions;

public static class LocationChangingExtensions
{
    public static async Task AskToLeavePage(
        this LocationChangingContext context,
        IJSRuntime runtime
    )
    {
        var isConfirmed = await runtime.InvokeAsync<bool>(
            "confirm",
            "You have unsaved changes. Are you sure you want to leave this page?"
        );

        if (!isConfirmed)
        {
            context.PreventNavigation();
        }
    }
}

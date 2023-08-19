using MudBlazor;
using StrawberryShake;

namespace K8Cloud.Web.Extensions;

public static class SnackbarExtensions
{
    public static void AddErrorFragments(this ISnackbar snackbar, IReadOnlyList<IErrorFragment> errors)
    {
        if (errors.Count == 0)
        {
            snackbar.Add("Unexpected error", Severity.Error);
            return;
        }

        foreach (var error in errors)
        {
            snackbar.Add(error.Message, Severity.Error);
        }
    }

    public static void AddClientErrors(this ISnackbar snackbar, IReadOnlyList<IClientError> errors)
    {
        if (errors.Count == 0)
        {
            snackbar.Add("Unexpected error", Severity.Error);
            return;
        }

        foreach (var error in errors)
        {
            snackbar.Add(error.Message, Severity.Error);
        }
    }
}

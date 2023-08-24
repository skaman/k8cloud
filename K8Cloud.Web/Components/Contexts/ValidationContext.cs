using AsyncAwaitBestPractices;
using K8Cloud.Web.Extensions;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace K8Cloud.Web.Components.Contexts;

public sealed class ValidationContext
{
    private readonly EditContext _editContext;
    private readonly ISnackbar _snackbar;
    private readonly ILogger _logger;
    private readonly ValidationMessageStore _validationMessageStore;
    private readonly Func<
        object,
        CancellationToken,
        Task<IValidationFragment?>
    > _validationCallback;
    private readonly Dictionary<string, object?> _validationStatus = new();

    private CancellationTokenSource? _cancellationTokenSource;

    public ValidationContext(
        EditContext editContext,
        ILogger logger,
        ISnackbar snackbar,
        Func<object, CancellationToken, Task<IValidationFragment?>> validationCallback
    )
    {
        _editContext = editContext;
        _logger = logger;
        _snackbar = snackbar;
        _validationMessageStore = new ValidationMessageStore(editContext);
        _validationCallback = validationCallback;

        _editContext.OnFieldChanged += HandleFieldChanged;
    }

    public async Task Validate()
    {
        await Validate(_editContext.Model, null);
    }

    public void UpdateValidation(IReadOnlyList<object> errors)
    {
        var validationError = errors
            .Where(x => x is IValidationErrorFragment)
            .Cast<IValidationErrorFragment>()
            .FirstOrDefault();
        if (validationError != null)
        {
            UpdateValidation(validationError);
        }
        else
        {
            var errorFragments = errors
                .Where(x => !(x is IErrorFragment))
                .Cast<IErrorFragment>()
                .ToList();
            _snackbar.AddErrorFragments(errorFragments);
        }
    }

    public void UpdateValidation(IValidationErrorFragment errorFragment)
    {
        _cancellationTokenSource?.Cancel();

        _validationMessageStore.Clear();
        foreach (var error in errorFragment.Errors!)
        {
            var identifier = new FieldIdentifier(_editContext.Model, error!.PropertyName!);
            _validationMessageStore.Add(identifier, error.ErrorMessage!);
        }
        _editContext.NotifyValidationStateChanged();
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        var modelValue = _editContext.Model
            .GetType()
            .GetProperty(e.FieldIdentifier.FieldName)
            ?.GetValue(_editContext.Model);
        if (
            _validationStatus.TryGetValue(e.FieldIdentifier.FieldName, out var value)
            && (
                value == null && modelValue == null || value != null && value.Equals(modelValue)
            )
        )
        {
            return;
        }

        _validationStatus[e.FieldIdentifier.FieldName] = modelValue;

        Validate(e.FieldIdentifier.Model, e.FieldIdentifier.FieldName)
            .SafeFireAndForget(ex =>
            {
                _logger.LogError(ex, "Error while validating form");
                _snackbar.Add("Unexpected error", Severity.Error);
            });
    }

    private async Task Validate(object model, string? propertyName)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        var validationResult = await _validationCallback.Invoke(
            model,
            _cancellationTokenSource.Token
        );
        if (validationResult == null)
        {
            _logger.LogError("Validation result can't be NULL");
            _snackbar.Add("Unexpected error", Severity.Error);
            return;
        }
        _validationMessageStore.Clear();

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors!)
            {
                if (propertyName != null && propertyName != error!.PropertyName)
                {
                    continue;
                }

                var identifier = new FieldIdentifier(model, error!.PropertyName!);
                _validationMessageStore.Add(identifier, error!.ErrorMessage!);
            }
        }

        _editContext.NotifyValidationStateChanged();
    }
}

using FluentValidation;
using FluentValidation.Validators;

namespace K8Cloud.Shared.Validators;

internal static class UrlValidator
{
    public static readonly string[] AllowedSchemes = { Uri.UriSchemeHttp, Uri.UriSchemeHttps };
    public static readonly string[] AllowedSchemesOnlyHttps = { Uri.UriSchemeHttps };
}

/// <summary>
/// Fluent validation rule for validating URLs.
/// </summary>
/// <typeparam name="T">Model type.</typeparam>
public class UrlValidator<T> : PropertyValidator<T, string>
{
    private readonly bool _onlyHttps;

    /// <inheritdoc />
    public override string Name => "UrlValidator";

    public UrlValidator(bool onlyHttps = false)
    {
        _onlyHttps = onlyHttps;
    }

    /// <inheritdoc />
    public override bool IsValid(ValidationContext<T> context, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        var isValid =
            Uri.TryCreate(value, UriKind.Absolute, out var uriResult)
            && IsValidScheme(uriResult.Scheme);
        if (!isValid)
        {
            context.AddFailure(
                context.PropertyPath,
                _onlyHttps
                    ? $"'{context.DisplayName}' must be a valid HTTPS URL"
                    : $"'{context.DisplayName}' must be a valid HTTP or HTTPS URL"
            );
        }
        return isValid;
    }

    private bool IsValidScheme(string scheme)
    {
        return _onlyHttps
            ? UrlValidator.AllowedSchemesOnlyHttps.Contains(scheme)
            : UrlValidator.AllowedSchemes.Contains(scheme);
    }
}

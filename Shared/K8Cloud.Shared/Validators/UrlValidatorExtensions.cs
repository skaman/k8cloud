using FluentValidation;

namespace K8Cloud.Shared.Validators;

/// <summary>
/// Extension methods for <see cref="Rfc1035Validator{T}"/>.
/// </summary>
public static class UrlValidatorExtensions
{
    /// <summary>
    /// Validates that the string is a valid URL.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="ruleBuilder">Rule builder.</param>
    /// <param name="onlyHttps">Only HTTPS URLs are allowed.</param>
    /// <returns>Rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> Url<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        bool onlyHttps = false
    )
    {
        return ruleBuilder.SetValidator(new UrlValidator<T>(onlyHttps));
    }
}

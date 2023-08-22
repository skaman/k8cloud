using FluentValidation;

namespace K8Cloud.Shared.Validators;

/// <summary>
/// Extension methods for <see cref="Rfc1123Validator{T}"/>.
/// </summary>
public static class Rfc1123ValidatorExtensions
{
    /// <summary>
    /// Validates that the string is a valid RFC 1123.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="ruleBuilder">Rule builder.</param>
    /// <returns>Rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> Rfc1123<T>(
        this IRuleBuilder<T, string> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(new Rfc1123Validator<T>());
    }
}

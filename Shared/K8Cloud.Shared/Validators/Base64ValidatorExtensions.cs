using FluentValidation;

namespace K8Cloud.Shared.Validators;

/// <summary>
/// Extension methods for <see cref="Base64Validator{T}"/>.
/// </summary>
public static class Base64ValidatorExtensions
{
    /// <summary>
    /// Validates that the string is a valid Base64 string.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="ruleBuilder">Rule builder.</param>
    /// <returns>Rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> Base64<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new Base64Validator<T>());
    }
}

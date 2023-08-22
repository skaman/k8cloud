using FluentValidation;

namespace K8Cloud.Shared.Validators;

/// <summary>
/// Extension methods for <see cref="DnsSubdomainValidator{T}"/>.
/// </summary>
public static class DnsSubdomainValidatorExtensions
{
    /// <summary>
    /// Validates that the string is a valid DNS subdomains.
    /// </summary>
    /// <typeparam name="T">Model type.</typeparam>
    /// <param name="ruleBuilder">Rule builder.</param>
    /// <returns>Rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> DnsSubdomain<T>(
        this IRuleBuilder<T, string> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(new DnsSubdomainValidator<T>());
    }
}

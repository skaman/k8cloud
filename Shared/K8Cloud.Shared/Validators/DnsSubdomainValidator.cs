using FluentValidation;
using FluentValidation.Validators;
using System.Text.RegularExpressions;

namespace K8Cloud.Shared.Validators;

/// <summary>
/// Fluent validation for DNS subdomains.
/// https://kubernetes.io/docs/concepts/overview/working-with-objects/names/#dns-subdomain-names
/// </summary>
/// <typeparam name="T"></typeparam>
public partial class DnsSubdomainValidator<T> : PropertyValidator<T, string>
{
    /// <inheritdoc />
    public override string Name => "DnsSubdomainValidator";

    /// <inheritdoc />
    public override bool IsValid(ValidationContext<T> context, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        if (value.Length > 253)
        {
            context.AddFailure(
                context.PropertyPath,
                $"'{context.DisplayName}' must contain at most 253 characters"
            );
            return false;
        }

        if (!ContainAlphanumericOrMinusOrUnderscoreRegex().IsMatch(value))
        {
            context.AddFailure(
                context.PropertyPath,
                $"'{context.DisplayName}' must contain only lowercase alphanumeric characters, '-' or '.'"
            );
            return false;
        }

        if (!StartWithAlphanumericRegex().IsMatch(value))
        {
            context.AddFailure(
                context.PropertyPath,
                $"'{context.DisplayName}' must start with an alphanumeric character"
            );
            return false;
        }

        if (!EndWithAlphanumericRegex().IsMatch(value))
        {
            context.AddFailure(
                context.PropertyPath,
                $"'{context.DisplayName}' must end with an alphanumeric character"
            );
            return false;
        }

        return true;
    }

    [GeneratedRegex(@"^[a-z0-9\-\.]*$", RegexOptions.Compiled)]
    private static partial Regex ContainAlphanumericOrMinusOrUnderscoreRegex();

    [GeneratedRegex(@"^[a-z0-9]", RegexOptions.Compiled)]
    private static partial Regex StartWithAlphanumericRegex();

    [GeneratedRegex(@"[a-z0-9]$", RegexOptions.Compiled)]
    private static partial Regex EndWithAlphanumericRegex();
}

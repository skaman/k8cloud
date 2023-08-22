using FluentValidation;
using FluentValidation.Validators;
using System.Text.RegularExpressions;

namespace K8Cloud.Shared.Validators;

/// <summary>
/// Fluent validation rule for RFC 1123.
/// https://kubernetes.io/docs/concepts/overview/working-with-objects/names/#dns-label-names
/// </summary>
/// <typeparam name="T">Model type.</typeparam>
public partial class Rfc1123Validator<T> : PropertyValidator<T, string>
{
    /// <inheritdoc />
    public override string Name => "Rfc1123Validator";

    /// <inheritdoc />
    public override bool IsValid(ValidationContext<T> context, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        if (value.Length > 63)
        {
            context.AddFailure(
                context.PropertyPath,
                $"'{context.DisplayName}' must contain at most 63 characters"
            );
            return false;
        }

        if (!ContainAlphanumericOrMinusRegex().IsMatch(value))
        {
            context.AddFailure(
                context.PropertyPath,
                $"'{context.DisplayName}' must contain only lowercase alphanumeric characters or '-'"
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

    [GeneratedRegex(@"^[a-z0-9\-]*$", RegexOptions.Compiled)]
    private static partial Regex ContainAlphanumericOrMinusRegex();

    [GeneratedRegex(@"^[a-z0-9]", RegexOptions.Compiled)]
    private static partial Regex StartWithAlphanumericRegex();

    [GeneratedRegex(@"[a-z0-9]$", RegexOptions.Compiled)]
    private static partial Regex EndWithAlphanumericRegex();
}

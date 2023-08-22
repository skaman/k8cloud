using FluentValidation;
using FluentValidation.Validators;

namespace K8Cloud.Shared.Validators;

/// <summary>
/// Fluent validation rule for base64 strings.
/// </summary>
/// <typeparam name="T">Model type.</typeparam>
public class Base64Validator<T> : PropertyValidator<T, string>
{
    /// <inheritdoc />
    public override string Name => "Base64Validator";

    /// <inheritdoc />
    public override bool IsValid(ValidationContext<T> context, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        var isValid = IsBase64String(value);
        if (!isValid)
        {
            context.AddFailure(
                context.PropertyPath,
                $"'{context.DisplayName}' must be a valid base64 string"
            );
        }
        return isValid;
    }

    private static bool IsBase64String(string base64)
    {
        Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer, out var _);
    }
}

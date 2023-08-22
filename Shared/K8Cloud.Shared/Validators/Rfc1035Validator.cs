namespace K8Cloud.Shared.Validators;

/// <summary>
/// Fluent validation rule for RFC 1035.
/// https://kubernetes.io/docs/concepts/overview/working-with-objects/names/#rfc-1035-label-names
/// </summary>
/// <typeparam name="T">Model type.</typeparam>
public class Rfc1035Validator<T> : Rfc1123Validator<T>
{
    /// <inheritdoc />
    public override string Name => "Rfc1035Validator";
}

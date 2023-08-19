namespace K8Cloud.Shared.Extensions;

public static class FluentValidatorExtensions
{
    public static bool TryGetValue<T>(
        this IDictionary<string, object> data,
        string key,
        out T? value
    )
    {
        if (data.TryGetValue(key, out var rawvalue) && rawvalue is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }
}

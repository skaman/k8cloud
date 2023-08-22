namespace K8Cloud.Shared.Extensions;

/// <summary>
/// Fluent validator extensions.
/// </summary>
public static class FluentValidatorExtensions
{
    /// <summary>
    /// Try to get a value from a dictionary.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="data">Dictionary data.</param>
    /// <param name="key">Requested key.</param>
    /// <param name="value">Output valute.</param>
    /// <returns>True if exists in the dictionary, otherwise false.</returns>
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

    /// <summary>
    /// Get a value from a dictionary.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="data">Dictionary data.</param>
    /// <param name="key">Requested key.</param>
    /// <returns>Value.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static T GetValue<T>(this IDictionary<string, object> data, string key)
    {
        if (data.TryGetValue(key, out var rawvalue) && rawvalue is T typedValue)
        {
            return typedValue;
        }

        throw new KeyNotFoundException(key);
    }
}

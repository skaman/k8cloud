using K8Cloud.Contracts.Kubernetes.RequestResponse;
using MassTransit;

namespace K8Cloud.Blazor.Utils;

public class Validator<T> where T : class
{
    private readonly Func<IRequestClient<ValidateProperty<T>>> _validatorAction;
    private readonly Func<object, string, ValidateProperty<T>> _buildModelAction;

    public Validator(
        Func<IRequestClient<ValidateProperty<T>>> validatorAction,
        Func<object, string, ValidateProperty<T>> buildModelAction
    )
    {
        _validatorAction = validatorAction;
        _buildModelAction = buildModelAction;
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue =>
        async (model, propertyName) =>
        {
            var client = _validatorAction();
            var result = await client.GetResponse<ValidatePropertyResponse>(
                _buildModelAction(model, propertyName)
            );
            return result.Message.IsValid
                ? Array.Empty<string>()
                : result.Message.Errors.Select(e => e.Message);
        };
}

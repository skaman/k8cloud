using FluentValidation;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.RequestResponse;
using MassTransit;

namespace K8Cloud.Shared.Consumers;

public abstract class ConsumerWithValidator<T>
    : IConsumer<T>,
        IConsumer<Validate<T>>,
        IConsumer<ValidateProperty<T>> where T : class
{
    private readonly AbstractValidator<T> _validator;

    protected ConsumerWithValidator(AbstractValidator<T> validator)
    {
        _validator = validator;
    }

    public async Task Consume(ConsumeContext<T> context)
    {
        var validationResult = await _validator
            .ValidateAsync(context.Message, context.CancellationToken)
            .ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            await context
                .RespondAsync(
                    new ValidationErrorResponse
                    {
                        Errors = validationResult.Errors
                            .Select(
                                x =>
                                    new ValidationError
                                    {
                                        Message = x.ErrorMessage,
                                        PropertyName = x.PropertyName
                                    }
                            )
                            .ToArray()
                    }
                )
                .ConfigureAwait(false);
            return;
        }

        await ConsumeValidated(context).ConfigureAwait(false);
    }

    public async Task Consume(ConsumeContext<Validate<T>> context)
    {
        var result = await _validator.ValidateAsync(context.Message.Data).ConfigureAwait(false);
        await context
            .RespondAsync(
                new ValidateResponse
                {
                    IsValid = result.IsValid,
                    Errors = result.Errors
                        .Select(
                            x =>
                                new ValidationError
                                {
                                    Message = x.ErrorMessage,
                                    PropertyName = x.PropertyName
                                }
                        )
                        .ToArray()
                }
            )
            .ConfigureAwait(false);
    }

    public async Task Consume(ConsumeContext<ValidateProperty<T>> context)
    {
        var result = await _validator
            .ValidateAsync(
                ValidationContext<T>.CreateWithOptions(
                    context.Message.Data,
                    x => x.IncludeProperties(context.Message.PropertyName)
                )
            )
            .ConfigureAwait(false);
        await context
            .RespondAsync(
                new ValidatePropertyResponse
                {
                    IsValid = result.IsValid,
                    Errors = result.Errors
                        .Select(
                            x =>
                                new ValidationError
                                {
                                    Message = x.ErrorMessage,
                                    PropertyName = x.PropertyName
                                }
                        )
                        .ToArray()
                }
            )
            .ConfigureAwait(false);
    }

    public abstract Task ConsumeValidated(ConsumeContext<T> context);
}

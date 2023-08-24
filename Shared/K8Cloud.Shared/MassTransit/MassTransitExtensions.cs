using K8Cloud.Contracts.Interfaces;
using MassTransit;

namespace K8Cloud.Shared.MassTransit;

public static class MassTransitExtensions
{
    public static IEventCorrelationConfigurator<TInstance, T> CorrelateByIdentifier<TInstance, T>(
        this IEventCorrelationConfigurator<TInstance, T> context
    )
        where TInstance : class, SagaStateMachineInstance
        where T : class, IEventWithResource<IResource>
    {
        return context.CorrelateById(x => x.Message.Resource.Id);
    }

    public static IEventCorrelationConfigurator<TInstance, T> RescheduleIfMissingInstance<
        TInstance,
        T
    >(this IEventCorrelationConfigurator<TInstance, T> context, TimeSpan delay)
        where TInstance : class, SagaStateMachineInstance
        where T : class
    {
        return context.OnMissingInstance(
            x =>
                x.ExecuteAsync(
                    context =>
                        context.SchedulePublish(delay, context.Message, context.CancellationToken)
                )
        );
    }
}

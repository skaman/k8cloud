using MassTransit;

namespace K8Cloud.Shared.MassTransit;

public abstract class SimpleStateMachineActivity<TSaga> : IStateMachineActivity<TSaga>
    where TSaga : class, ISaga
{
    public abstract string ScopeName { get; }

    /// <inheritdoc />
    public void Probe(ProbeContext context)
    {
        context.CreateScope(ScopeName);
    }

    /// <inheritdoc />
    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    /// <inheritdoc />
    public async Task Execute(BehaviorContext<TSaga> context, IBehavior<TSaga> next)
    {
        await Process(context).ConfigureAwait(false);
        await next.Execute(context).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task Execute<T>(BehaviorContext<TSaga, T> context, IBehavior<TSaga, T> next)
        where T : class
    {
        await Process(context).ConfigureAwait(false);
        await next.Execute(context).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task Faulted<TException>(
        BehaviorExceptionContext<TSaga, TException> context,
        IBehavior<TSaga> next
    )
        where TException : Exception
    {
        return next.Faulted(context);
    }

    /// <inheritdoc />
    public Task Faulted<T, TException>(
        BehaviorExceptionContext<TSaga, T, TException> context,
        IBehavior<TSaga, T> next
    )
        where T : class
        where TException : Exception
    {
        return next.Faulted(context);
    }

    /// <summary>
    /// Process the activity.
    /// </summary>
    /// <param name="context">Behaviour context.</param>
    protected abstract Task Process(BehaviorContext<TSaga> context);
}

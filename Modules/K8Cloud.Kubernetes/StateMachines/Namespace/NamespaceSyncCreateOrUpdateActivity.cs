using K8Cloud.Kubernetes.Exceptions;
using K8Cloud.Kubernetes.Services;
using MassTransit;

namespace K8Cloud.Kubernetes.StateMachines.Namespace;

internal class NamespaceSyncCreateOrUpdateActivity : IStateMachineActivity<NamespaceSyncState>
{
    private readonly KubernetesService _kubernetesService;

    public NamespaceSyncCreateOrUpdateActivity(KubernetesService kubernetesService)
    {
        _kubernetesService = kubernetesService;
    }

    /// <inheritdoc />
    public void Probe(ProbeContext context)
    {
        context.CreateScope("namespace-sync");
    }

    /// <inheritdoc />
    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    /// <inheritdoc />
    public async Task Execute(
        BehaviorContext<NamespaceSyncState> context,
        IBehavior<NamespaceSyncState> next
    )
    {
        await Process(context).ConfigureAwait(false);
        await next.Execute(context).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task Execute<T>(
        BehaviorContext<NamespaceSyncState, T> context,
        IBehavior<NamespaceSyncState, T> next
    )
        where T : class
    {
        await Process(context).ConfigureAwait(false);
        await next.Execute(context).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task Faulted<TException>(
        BehaviorExceptionContext<NamespaceSyncState, TException> context,
        IBehavior<NamespaceSyncState> next
    )
        where TException : Exception
    {
        return next.Faulted(context);
    }

    /// <inheritdoc />
    public Task Faulted<T, TException>(
        BehaviorExceptionContext<NamespaceSyncState, T, TException> context,
        IBehavior<NamespaceSyncState, T> next
    )
        where T : class
        where TException : Exception
    {
        return next.Faulted(context);
    }

    private async Task Process(BehaviorContext<NamespaceSyncState> context)
    {
        if (context.Saga.ResourceToSync == null)
        {
            return;
        }

        try
        {
            await _kubernetesService
                .CreateOrUpdateNamespaceAsync(context.Saga.ResourceToSync)
                .ConfigureAwait(false);
            context.Saga.VersionSynced = context.Saga.ResourceToSync.Version;
            context.Saga.UpdatedAtSynced = context.Saga.ResourceToSync.UpdatedAt;
            context.Saga.ResourceToSync = null;
            context.Saga.RetryCount = 0;
            context.Saga.ErrorCode = null;
            context.Saga.ErrorMessage = null;
        }
        catch (KubernetesException ex)
        {
            context.Saga.RetryCount++;
            context.Saga.ErrorCode = ex.Status.Code;
            context.Saga.ErrorMessage = ex.Status.Message;
        }
    }
}

using AutoMapper;
using AutoMapper.QueryableExtensions;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Kubernetes.Exceptions;
using K8Cloud.Kubernetes.Extensions;
using K8Cloud.Kubernetes.Services;
using K8Cloud.Shared.Utils;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace K8Cloud.Kubernetes.StateMachines.Namespace;

internal class NamespaceSyncActivity : IStateMachineActivity<NamespaceSyncState>
{
    private const int MaxRetryCount = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly KubernetesService _kubernetesService;
    private readonly IMapper _mapper;

    public NamespaceSyncActivity(KubernetesService kubernetesService, IMapper mapper)
    {
        _kubernetesService = kubernetesService;
        _mapper = mapper;
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
    ) where T : class
    {
        await Process(context).ConfigureAwait(false);
        await next.Execute(context).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task Faulted<TException>(
        BehaviorExceptionContext<NamespaceSyncState, TException> context,
        IBehavior<NamespaceSyncState> next
    ) where TException : Exception
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

        var resource = context.Saga.ResourceToSync;

        try
        {
            await _kubernetesService
                .CreateOrUpdateNamespaceAsync(context.Saga.ResourceToSync)
                .ConfigureAwait(false);
            context.AddOrUpdatePayload(() => resource, _ => resource);
        }
        catch (KubernetesException ex)
        {
            context.AddOrUpdatePayload(() => ex.Status, _ => ex.Status);

            if (context.Saga.RetryCount > MaxRetryCount)
            {
                await context.SchedulePublish(
                    ExponentialRetry.GetDelay(RetryDelay, context.Saga.RetryCount),
                    new NamespaceSyncError { Resource = resource, Status = ex.Status }
                );
            }
            else
            {
                await context.SchedulePublish(
                    TimeSpan.FromSeconds(5),
                    new NamespaceSyncRetry { Resource = resource }
                );
            }
        }
    }
}

using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Events;
using MassTransit;

namespace K8Cloud.Kubernetes.StateMachines.Namespace;

internal class NamespaceSyncStateMachine : MassTransitStateMachine<NamespaceSyncState>
{
    private const int MaxRetryCount = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    public State Idle { get; private set; } = default!;
    public State Syncing { get; private set; } = default!;
    public State SyncError { get; private set; } = default!;

    public Event<NamespaceCreated> NamespaceCreated { get; private set; } = default!;
    public Event<NamespaceUpdated> NamespaceUpdated { get; private set; } = default!;
    public Event<NamespaceDeleted> NamespaceDeleted { get; private set; } = default!;
    public Event<NamespaceSync> NamespaceSync { get; private set; } = default!;

    //public Event<NamespaceSyncRetry> NamespaceSyncRetry { get; private set; } = default!;

    public Schedule<NamespaceSyncState, NamespaceSyncRetry> NamespaceSyncRetry
    {
        get;
        private set;
    } = default!;

    //public Schedule<ClusterState, RequestClusterStatus> RequestClusterStatus { get; private set; } =
    //    null!;

    public NamespaceSyncStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => NamespaceCreated, e => e.CorrelateById(x => x.Message.Resource.Id));
        Event(() => NamespaceUpdated, e => e.CorrelateById(x => x.Message.Resource.Id));
        Event(() => NamespaceDeleted, e => e.CorrelateById(x => x.Message.Resource.Id));
        Event(() => NamespaceSync, e => e.CorrelateById(x => x.Message.Resource.Id));

        Schedule(
            () => NamespaceSyncRetry,
            instance => instance.NamespaceSyncRetryTokenId,
            s =>
            {
                s.Delay = RetryDelay;
                s.Received = r => r.CorrelateById(x => x.Message.Resource.Id);
            }
        );

        SetCompletedWhenFinalized();

        Initially(When(NamespaceCreated).TransitionTo(Syncing));

        During(
            Idle,
            SyncError,
            When(NamespaceUpdated).ClearErrors().ClearRetryCount().TransitionTo(Syncing),
            When(NamespaceSync).ClearErrors().ClearRetryCount().TransitionTo(Syncing),
            When(NamespaceSyncRetry).TransitionTo(Syncing)
        );
        During(Syncing, Ignore(NamespaceUpdated));

        DuringAny(When(NamespaceDeleted).Finalize());

        WhenEnter(
            Syncing,
            b =>
                b.TrySync()
                    .IfElse(
                        x => x.IsSyncSuccess(),
                        x =>
                            x.UpdateResourceVersion()
                                .ClearErrors()
                                .ClearRetryCount()
                                .TransitionTo(Idle),
                        x =>
                            x.UpdateErrors()
                                .UpdateRetryCount()
                                .IfElse(x => x.Saga.RetryCount > MaxRetryCount, x => x, x => x)
                                .TransitionTo(SyncError)
                    )
        );
    }
}

internal static class NamespaceSyncStateMachineExtensions
{
    public static EventActivityBinder<NamespaceSyncState> TrySync(
        this EventActivityBinder<NamespaceSyncState> context
    )
    {
        return context.Activity(x => x.OfType<NamespaceSyncCreateOrUpdateActivity>());
    }

    public static bool IsSyncSuccess(this BehaviorContext<NamespaceSyncState> context)
    {
        return context.TryGetPayload<NamespaceResource>(out var _);
    }

    public static EventActivityBinder<NamespaceSyncState> UpdateResourceVersion(
        this EventActivityBinder<NamespaceSyncState> context
    )
    {
        return context.Then(x =>
        {
            var resource = x.GetPayload<NamespaceResource>();
            x.Saga.ResourceVersionSynced = resource.Version;
        });
    }

    public static EventActivityBinder<NamespaceSyncState> UpdateRetryCount(
        this EventActivityBinder<NamespaceSyncState> context
    )
    {
        return context.Then(x =>
        {
            x.Saga.RetryCount++;
        });
    }

    public static EventActivityBinder<NamespaceSyncState> ClearRetryCount(
        this EventActivityBinder<NamespaceSyncState> context
    )
    {
        return context.Then(x =>
        {
            x.Saga.RetryCount = 0;
        });
    }

    public static EventActivityBinder<NamespaceSyncState, T> ClearRetryCount<T>(
        this EventActivityBinder<NamespaceSyncState, T> context
    )
        where T : class
    {
        return context.Then(x =>
        {
            x.Saga.RetryCount = 0;
        });
    }

    public static EventActivityBinder<NamespaceSyncState> UpdateErrors(
        this EventActivityBinder<NamespaceSyncState> context
    )
    {
        return context.Then(x =>
        {
            var status = x.GetPayload<Status>();
            x.Saga.ErrorCode = status.Code;
            x.Saga.ErrorMessage = status.Message;
        });
    }

    public static EventActivityBinder<NamespaceSyncState> ClearErrors(
        this EventActivityBinder<NamespaceSyncState> context
    )
    {
        return context.Then(x =>
        {
            x.Saga.ErrorCode = null;
            x.Saga.ErrorMessage = null;
        });
    }

    public static EventActivityBinder<NamespaceSyncState, T> ClearErrors<T>(
        this EventActivityBinder<NamespaceSyncState, T> context
    )
        where T : class
    {
        return context.Then(x =>
        {
            x.Saga.ErrorCode = null;
            x.Saga.ErrorMessage = null;
        });
    }
}

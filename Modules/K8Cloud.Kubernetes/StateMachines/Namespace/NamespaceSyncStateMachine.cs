using K8Cloud.Contracts.Kubernetes.Enums;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Shared.MassTransit;
using MassTransit;

namespace K8Cloud.Kubernetes.StateMachines.Namespace;

// csharpier-ignore-start
internal class NamespaceSyncStateMachine : MassTransitStateMachine<NamespaceSyncState>
{
    private const int MaxRetryCount = 10;
    private static readonly TimeSpan RedeliverDelay = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(300);

    public State Synced { get; private set; } = default!;
    public State Syncing { get; private set; } = default!;
    public State SyncError { get; private set; } = default!;
    public State Deleted { get; private set; } = default!;

    public Event<NamespaceSync> NamespaceSync { get; private set; } = default!;
    public Event<NamespaceSyncRetry> NamespaceSyncRetry { get; private set; } = default!;
    public Event<NamespaceDeployCompleted> NamespaceDeployCompleted { get; private set; } = default!;
    public Event<NamespaceDeployFailed> NamespaceDeployFailed { get; private set; } = default!;

    public Schedule<NamespaceSyncState, NamespaceDeployTimeout> NamespaceDeployTimeout { get; private set; } = default!;

    public NamespaceSyncStateMachine()
    {
        // state field definition
        InstanceState(x => x.CurrentState);

        // events definition
        Event(() => NamespaceSync,            e => e.CorrelateByIdentifier());
        Event(() => NamespaceSyncRetry,       e => e.CorrelateByIdentifier());
        Event(() => NamespaceDeployCompleted, e => e.CorrelateByIdentifier());
        Event(() => NamespaceDeployFailed,    e => e.CorrelateByIdentifier());

        // schedulers definition
        Schedule(
            () => NamespaceDeployTimeout,
            instance => instance.NamespaceDeployTimeoutTokenId,
            s =>
            {
                s.Delay = Timeout;
                s.Received = r => r.CorrelateByIdentifier();
            }
        );

        // options
        SetCompletedWhenFinalized();

        // initiate saga
        Initially(
            When(NamespaceSync)
                .SaveInSyncResouceTime()
                .PublishNamespaceDeploy()
                .ScheduleNamespaceDeployTimeout(NamespaceDeployTimeout)
                .TransitionTo(Syncing)
        );

        // logic
        During(
            Synced,
            SyncError,

            When(NamespaceSync)
                .If(
                    x => x.CheckIfResourceIsNewer(),
                    x => x.ClearErrors()
                          .ClearRetryCount()
                          .SaveInSyncResouceTime()
                          .Unschedule(NamespaceDeployTimeout)
                          .PublishNamespaceDeploy()
                          .ScheduleNamespaceDeployTimeout(NamespaceDeployTimeout)
                          .TransitionTo(Syncing)
                ),

            When(NamespaceSyncRetry)
                .If(
                    x => x.CheckIfResourceIsNewer(),
                    x => x.SaveInSyncResouceTime()
                          .Unschedule(NamespaceDeployTimeout)
                          .PublishNamespaceDeploy()
                          .ScheduleNamespaceDeployTimeout(NamespaceDeployTimeout)
                          .TransitionTo(Syncing)
                )
        );

        During(
            Syncing,

            When(NamespaceSync)
                .If(
                    x => x.CheckIfResourceIsNewer(),
                    x => x.Reschedule(RedeliverDelay)
                ),

            When(NamespaceSyncRetry)
                .If(
                    x => x.CheckIfResourceIsNewer(),
                    x => x.Reschedule(RedeliverDelay)
                ),

            When(NamespaceDeployCompleted)
                .If(
                    x => x.Saga.InSyncResouceTime == x.Message.Resource.UpdatedAt,
                    x => x.SaveSyncedResouceTime()
                          .ClearInSyncResouceTime()
                          .ClearErrors()
                          .ClearRetryCount()
                          .Unschedule(NamespaceDeployTimeout)
                          .IfElse(
                              x => x.Message.DeployType == DeployType.Delete,
                              x => x.TransitionTo(Synced),
                              x => x.TransitionTo(Deleted)
                          )
                ),

            When(NamespaceDeployFailed)
                .IncreaseRetryCount()
                .SaveErrorResult()
                .Unschedule(NamespaceDeployTimeout)
                .TransitionTo(SyncError)
                .IfElse(
                    x => x.Saga.RetryCount > MaxRetryCount,
                    x => x.PublishNamespaceSyncFailed()
                          .If(
                              x => x.Message.DeployType == DeployType.Delete,
                              x => x.TransitionTo(Deleted)
                          ),
                    x => x.PublishScheduledNamespaceSyncRetry(RedeliverDelay)
                ),

            When(NamespaceDeployTimeout.Received)
                .IncreaseRetryCount()
                .SetEventTimeoutError()
                .Unschedule(NamespaceDeployTimeout)
                .TransitionTo(SyncError)
                .IfElse(
                    x => x.Saga.RetryCount > MaxRetryCount,
                    x => x.PublishNamespaceSyncFailed(),
                    x => x.PublishScheduledNamespaceSyncRetry(RedeliverDelay)
                )
        );

        WhenEnter(Deleted, x => x.Finalize());
    }
    // csharpier-ignore-end
}

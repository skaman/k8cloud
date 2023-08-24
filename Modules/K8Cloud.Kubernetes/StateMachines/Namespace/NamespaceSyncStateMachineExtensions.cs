using K8Cloud.Contracts.Interfaces;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.Events;
using K8Cloud.Shared.Utils;
using MassTransit;
using System.Net;

namespace K8Cloud.Kubernetes.StateMachines.Namespace;

internal static class NamespaceSyncStateMachineExtensions
{
    public static EventActivityBinder<NamespaceSyncState, NamespaceSync> PublishNamespaceDeploy(
        this EventActivityBinder<NamespaceSyncState, NamespaceSync> context
    )
    {
        return context.PublishAsync(
            context =>
                context.Init<NamespaceDeploy>(
                    new NamespaceDeploy
                    {
                        DeployType = context.Message.DeployType,
                        Resource = context.Message.Resource,
                        Timestamp = DateTime.UtcNow
                    }
                )
        );
    }

    public static EventActivityBinder<
        NamespaceSyncState,
        NamespaceSyncRetry
    > PublishNamespaceDeploy(
        this EventActivityBinder<NamespaceSyncState, NamespaceSyncRetry> context
    )
    {
        return context.PublishAsync(
            context =>
                context.Init<NamespaceDeploy>(
                    new NamespaceDeploy
                    {
                        DeployType = context.Message.DeployType,
                        Resource = context.Message.Resource,
                        Timestamp = DateTime.UtcNow
                    }
                )
        );
    }

    public static EventActivityBinder<
        NamespaceSyncState,
        NamespaceDeployFailed
    > PublishScheduledNamespaceSyncRetry(
        this EventActivityBinder<NamespaceSyncState, NamespaceDeployFailed> context,
        TimeSpan baseDelay
    )
    {
        return context.ThenAsync(async x =>
        {
            var delay = ExponentialRetry.GetDelay(baseDelay, x.Saga.RetryCount);
            await x.SchedulePublish(
                delay,
                new NamespaceSyncRetry
                {
                    DeployType = x.Message.DeployType,
                    Resource = x.Message.Resource,
                    Timestamp = DateTime.UtcNow
                },
                x.CancellationToken
            );
        });
    }

    public static EventActivityBinder<
        NamespaceSyncState,
        NamespaceDeployTimeout
    > PublishScheduledNamespaceSyncRetry(
        this EventActivityBinder<NamespaceSyncState, NamespaceDeployTimeout> context,
        TimeSpan baseDelay
    )
    {
        return context.ThenAsync(async x =>
        {
            var delay = ExponentialRetry.GetDelay(baseDelay, x.Saga.RetryCount);
            await x.SchedulePublish(
                delay,
                new NamespaceSyncRetry
                {
                    DeployType = x.Message.DeployType,
                    Resource = x.Message.Resource,
                    Timestamp = DateTime.UtcNow
                },
                x.CancellationToken
            );
        });
    }

    public static EventActivityBinder<
        NamespaceSyncState,
        NamespaceDeployFailed
    > PublishNamespaceSyncFailed(
        this EventActivityBinder<NamespaceSyncState, NamespaceDeployFailed> context
    )
    {
        return context.PublishAsync(
            context =>
                context.Init<NamespaceSyncFailed>(
                    new NamespaceSyncFailed
                    {
                        DeployType = context.Message.DeployType,
                        Resource = context.Message.Resource,
                        Status = context.Message.Status,
                        Timestamp = DateTime.UtcNow
                    }
                )
        );
    }

    public static EventActivityBinder<
        NamespaceSyncState,
        NamespaceDeployTimeout
    > PublishNamespaceSyncFailed(
        this EventActivityBinder<NamespaceSyncState, NamespaceDeployTimeout> context
    )
    {
        return context.PublishAsync(
            context =>
                context.Init<NamespaceSyncFailed>(
                    new NamespaceSyncFailed
                    {
                        DeployType = context.Message.DeployType,
                        Resource = context.Message.Resource,
                        Status = new Status
                        {
                            Code = HttpStatusCode.RequestTimeout,
                            Message = "Internal event timeout"
                        },
                        Timestamp = DateTime.UtcNow
                    }
                )
        );
    }

    public static EventActivityBinder<NamespaceSyncState, T> Reschedule<T>(
        this EventActivityBinder<NamespaceSyncState, T> context,
        TimeSpan delay
    )
        where T : class
    {
        return context.ThenAsync(async x =>
        {
            await x.SchedulePublish(delay, x.Message, x.CancellationToken);
        });
    }

    public static EventActivityBinder<
        NamespaceSyncState,
        NamespaceSync
    > ScheduleNamespaceDeployTimeout(
        this EventActivityBinder<NamespaceSyncState, NamespaceSync> context,
        Schedule<NamespaceSyncState, NamespaceDeployTimeout> schedule
    )
    {
        return context.Schedule(
            schedule,
            context =>
                context.Init<NamespaceDeployTimeout>(
                    new NamespaceDeployTimeout
                    {
                        DeployType = context.Message.DeployType,
                        Resource = context.Message.Resource,
                        Timestamp = DateTime.UtcNow
                    }
                )
        );
    }

    public static EventActivityBinder<
        NamespaceSyncState,
        NamespaceSyncRetry
    > ScheduleNamespaceDeployTimeout(
        this EventActivityBinder<NamespaceSyncState, NamespaceSyncRetry> context,
        Schedule<NamespaceSyncState, NamespaceDeployTimeout> schedule
    )
    {
        return context.Schedule(
            schedule,
            context =>
                context.Init<NamespaceDeployTimeout>(
                    new NamespaceDeployTimeout
                    {
                        DeployType = context.Message.DeployType,
                        Resource = context.Message.Resource,
                        Timestamp = DateTime.UtcNow
                    }
                )
        );
    }

    public static bool CheckIfResourceIsNewer<T>(
        this BehaviorContext<NamespaceSyncState, T> context
    )
        where T : class, IEventWithResource<NamespaceResource>
    {
        return (
                context.Saga.SyncedResouce == null
                || (
                    context.Saga.SyncedResouce.UpdatedAt < context.Message.Resource.UpdatedAt
                    && context.Saga.SyncedResouce.Version != context.Message.Resource.Version
                )
            )
            && (
                context.Saga.InSyncResouce == null
                || (
                    context.Saga.InSyncResouce.UpdatedAt < context.Message.Resource.UpdatedAt
                    && context.Saga.InSyncResouce.Version != context.Message.Resource.Version
                )
            );
    }

    public static EventActivityBinder<NamespaceSyncState, T> SaveInSyncResouce<T>(
        this EventActivityBinder<NamespaceSyncState, T> context
    )
        where T : class, IEventWithResource<NamespaceResource>
    {
        return context.Then(x =>
        {
            x.Saga.InSyncResouce = x.Message.Resource;
        });
    }

    public static EventActivityBinder<NamespaceSyncState, T> SaveSyncedResouce<T>(
        this EventActivityBinder<NamespaceSyncState, T> context
    )
        where T : class, IEventWithResource<NamespaceResource>
    {
        return context.Then(x =>
        {
            x.Saga.SyncedResouce = x.Message.Resource;
        });
    }

    public static EventActivityBinder<NamespaceSyncState, NamespaceDeployFailed> SaveErrorResult(
        this EventActivityBinder<NamespaceSyncState, NamespaceDeployFailed> context
    )
    {
        return context.Then(x =>
        {
            x.Saga.ErrorStatus = x.Message.Status;
        });
    }

    public static EventActivityBinder<
        NamespaceSyncState,
        NamespaceDeployTimeout
    > SetEventTimeoutError(
        this EventActivityBinder<NamespaceSyncState, NamespaceDeployTimeout> context
    )
    {
        return context.Then(x =>
        {
            x.Saga.ErrorStatus = new Status
            {
                Code = HttpStatusCode.RequestTimeout,
                Message = "Internal event timeout"
            };
        });
    }

    public static EventActivityBinder<NamespaceSyncState, T> ClearInSyncResouce<T>(
        this EventActivityBinder<NamespaceSyncState, T> context
    )
        where T : class, IEventWithResource<NamespaceResource>
    {
        return context.Then(x =>
        {
            x.Saga.InSyncResouce = null;
        });
    }

    public static EventActivityBinder<NamespaceSyncState, T> IncreaseRetryCount<T>(
        this EventActivityBinder<NamespaceSyncState, T> context
    )
        where T : class
    {
        return context.Then(x =>
        {
            x.Saga.RetryCount++;
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

    public static EventActivityBinder<NamespaceSyncState, T> ClearErrorStatus<T>(
        this EventActivityBinder<NamespaceSyncState, T> context
    )
        where T : class
    {
        return context.Then(x =>
        {
            x.Saga.ErrorStatus = null;
        });
    }

    public static EventActivityBinder<NamespaceSyncState> ClearAll(
        this EventActivityBinder<NamespaceSyncState> context
    )
    {
        return context.Then(x =>
        {
            x.Saga.RetryCount = 0;
            x.Saga.ErrorStatus = null;
            x.Saga.InSyncResouce = null;
            x.Saga.SyncedResouce = null;
        });
    }
}

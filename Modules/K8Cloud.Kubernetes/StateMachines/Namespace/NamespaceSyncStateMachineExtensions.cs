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
                context.Saga.SyncedResouceTime == null
                || context.Saga.SyncedResouceTime < context.Message.Resource.UpdatedAt
            )
            && (
                context.Saga.InSyncResouceTime == null
                || context.Saga.InSyncResouceTime < context.Message.Resource.UpdatedAt
            );
    }

    public static EventActivityBinder<NamespaceSyncState, T> SaveInSyncResouceTime<T>(
        this EventActivityBinder<NamespaceSyncState, T> context
    )
        where T : class, IEventWithResource<NamespaceResource>
    {
        return context.Then(x =>
        {
            x.Saga.InSyncResouceTime = x.Message.Resource.UpdatedAt;
        });
    }

    public static EventActivityBinder<NamespaceSyncState, T> SaveSyncedResouceTime<T>(
        this EventActivityBinder<NamespaceSyncState, T> context
    )
        where T : class, IEventWithResource<NamespaceResource>
    {
        return context.Then(x =>
        {
            x.Saga.SyncedResouceTime = x.Message.Resource.UpdatedAt;
        });
    }

    public static EventActivityBinder<NamespaceSyncState, NamespaceDeployFailed> SaveErrorResult(
        this EventActivityBinder<NamespaceSyncState, NamespaceDeployFailed> context
    )
    {
        return context.Then(x =>
        {
            x.Saga.ErrorCode = x.Message.Status.Code;
            x.Saga.ErrorMessage = x.Message.Status.Message;
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
            x.Saga.ErrorCode = HttpStatusCode.RequestTimeout;
            x.Saga.ErrorMessage = "Namespace deploy timeout";
        });
    }

    public static EventActivityBinder<NamespaceSyncState, T> ClearInSyncResouceTime<T>(
        this EventActivityBinder<NamespaceSyncState, T> context
    )
        where T : class, IEventWithResource<NamespaceResource>
    {
        return context.Then(x =>
        {
            x.Saga.InSyncResouceTime = null;
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

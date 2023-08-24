using K8Cloud.Contracts.Kubernetes.Data;
using MassTransit;
using System.Net;

namespace K8Cloud.Kubernetes.StateMachines.Namespace;

internal class NamespaceSyncState : SagaStateMachineInstance
{
    public required Guid CorrelationId { get; set; }
    public required string CurrentState { get; set; }
    public NamespaceResource? SyncedResouce { get; set; }
    public NamespaceResource? InSyncResouce { get; set; }
    public int RetryCount { get; set; }
    public Status? ErrorStatus { get; set; }
    public Guid? NamespaceDeployTimeoutTokenId { get; set; }
}

using MassTransit;
using System.Net;

namespace K8Cloud.Kubernetes.StateMachines.Namespace;

internal class NamespaceSyncState : SagaStateMachineInstance
{
    public required Guid CorrelationId { get; set; }
    public required string CurrentState { get; set; }
    public DateTime? SyncedResouceTime { get; set; }
    public DateTime? InSyncResouceTime { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public HttpStatusCode? ErrorCode { get; set; }
    public Guid? NamespaceDeployTimeoutTokenId { get; set; }
}

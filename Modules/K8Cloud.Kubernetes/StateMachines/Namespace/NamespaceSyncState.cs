using K8Cloud.Contracts.Kubernetes.Data;
using MassTransit;
using System.Net;

namespace K8Cloud.Kubernetes.StateMachines.Namespace;

internal class NamespaceSyncState : SagaStateMachineInstance
{
    public required Guid CorrelationId { get; set; }
    public required string CurrentState { get; set; }
    public string? VersionSynced { get; set; }
    public DateTime? UpdatedAtSynced { get; set; }
    public NamespaceResource? ResourceToSync { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public HttpStatusCode? ErrorCode { get; set; }
    public Guid? NamespaceSyncRetryTokenId { get; set; }
}

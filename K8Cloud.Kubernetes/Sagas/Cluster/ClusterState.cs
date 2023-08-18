using MassTransit;

namespace K8Cloud.Kubernetes.Sagas.Cluster;

internal class ClusterState : SagaStateMachineInstance
{
    public required Guid CorrelationId { get; set; }
    public required string CurrentState { get; set; }
    public required string ServerName { get; set; }
    public required string ServerAddress { get; set; }
    public required string ServerCertificateAuthorityData { get; set; }
    public required string UserName { get; set; }
    public required string UserCredentialsCertificateData { get; set; }
    public required string UserCredentialsKeyData { get; set; }
    public required string Namespace { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdateAt { get; set; }

    public Guid? RequestClusterStatusTokenId { get; set; }
}

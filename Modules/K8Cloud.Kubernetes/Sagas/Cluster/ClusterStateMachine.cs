using K8Cloud.Contracts.Kubernetes.Events;
using MassTransit;

namespace K8Cloud.Kubernetes.Sagas.Cluster;

internal class ClusterStateMachine : MassTransitStateMachine<ClusterState>
{
    public State Created { get; private set; } = null!;
    public State Healthy { get; private set; } = null!;
    public State NotHealthy { get; private set; } = null!;

    //public Event<AddCluster> CreateCluster { get; private set; } = null!;

    //public Schedule<ClusterState, RequestClusterStatus> RequestClusterStatus { get; private set; } =
    //    null!;

    public ClusterStateMachine()
    {
        InstanceState(x => x.CurrentState);

        //Event(() => CreateCluster, e => e.CorrelateById(x => x.Message.Id));

        //Schedule(
        //    () => RequestClusterStatus,
        //    instance => instance.RequestClusterStatusTokenId,
        //    s =>
        //    {
        //        s.Delay = TimeSpan.FromDays(30);
        //    }
        //);

        //Initially(When(CreateCluster).CopyData().UpdateCreatedAt().TransitionTo(Created));

        //WhenEnter(
        //    Created,
        //    binder =>
        //        binder.Schedule(
        //            RequestClusterStatus,
        //            x => x.Init<RequestClusterStatus>(new { x.CorrelationId })
        //        )
        //);
    }
}

internal static class ClusterStateMachineExtensions
{
    //public static EventActivityBinder<ClusterState, T> CopyData<T>(
    //    this EventActivityBinder<ClusterState, T> binder
    //) where T : class, IClusterInformationData
    //{
    //    return binder.Then(x =>
    //    {
    //        x.Saga.ServerName = x.Message.Data.ServerName;
    //        x.Saga.ServerAddress = x.Message.Data.ServerAddress;
    //        x.Saga.ServerCertificateAuthorityData = x.Message.Data.ServerCertificateAuthorityData;
    //        x.Saga.UserName = x.Message.Data.UserName;
    //        x.Saga.UserCredentialsCertificateData = x.Message.Data.UserCredentialsCertificateData;
    //        x.Saga.UserCredentialsKeyData = x.Message.Data.UserCredentialsKeyData;
    //        x.Saga.Namespace = x.Message.Data.Namespace;
    //    });
    //}

    public static EventActivityBinder<ClusterState, T> UpdateCreatedAt<T>(
        this EventActivityBinder<ClusterState, T> binder
    ) where T : class
    {
        return binder.Then(x =>
        {
            x.Saga.CreatedAt = DateTime.UtcNow;
        });
    }
}

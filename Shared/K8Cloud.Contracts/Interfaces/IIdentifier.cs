using K8Cloud.Contracts.Kubernetes.Enums;

namespace K8Cloud.Contracts.Interfaces;

public interface IIdentifier
{
    public Guid Id { get; }
}

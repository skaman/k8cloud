namespace K8Cloud.Contracts.Interfaces;

public interface IEventWithResource<out T>
    where T : class, IResource
{
    public T Resource { get; }
}

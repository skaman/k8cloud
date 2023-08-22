using HotChocolate.Execution;

namespace K8Cloud.Shared.GraphQL.Exceptions;

public class ResourceNotFoundException : QueryException
{
    public ResourceNotFoundException(Guid resourceId) : base($"Resource with id {resourceId} not found.")
    {
        ResourceId = resourceId;
    }

    public Guid ResourceId { get; }
}

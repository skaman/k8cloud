using HotChocolate.Execution;

namespace K8Cloud.Shared.GraphQL.Exceptions;

/// <summary>
/// Resource not found exception.
/// </summary>
public class ResourceNotFoundException : QueryException
{
    public ResourceNotFoundException(Guid resourceId)
        : base($"Resource with id {resourceId} not found.")
    {
        ResourceId = resourceId;
    }

    /// <summary>
    /// Resource ID.
    /// </summary>
    public Guid ResourceId { get; }
}

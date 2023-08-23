using System.Net;

namespace K8Cloud.Contracts.Kubernetes.Data;

public record Status
{
    public required HttpStatusCode Code { get; init; }
    public required string Message { get; init; }
}

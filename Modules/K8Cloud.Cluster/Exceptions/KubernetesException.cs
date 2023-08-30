using K8Cloud.Contracts.Kubernetes.Data;
using k8s;
using k8s.Models;
using System.Net;

namespace K8Cloud.Cluster.Exceptions;

public class KubernetesException : Exception
{
    public KubernetesException(Status status) : base(status.Message)
    {
        Status = status;
    }

    public KubernetesException(string? message, Status status) : base(message)
    {
        Status = status;
    }

    public KubernetesException(string? message, Status status, Exception? innerException)
        : base(message, innerException)
    {
        Status = status;
    }

    public Status Status { get; }

    public static KubernetesException FromException(Exception e)
    {
        if (
            e is k8s.Autorest.HttpOperationException httpOperationException
            && TryDeserializeStatus(httpOperationException.Response.Content, out var status)
        )
        {
            throw new KubernetesException(
                e.Message,
                new Status
                {
                    Code = httpOperationException.Response.StatusCode,
                    Message = status!.Message
                },
                e
            );
        }

        if (e is k8s.KubernetesException kubernetesException)
        {
            return new KubernetesException(
                kubernetesException.Message,
                new Status
                {
                    Code =
                        kubernetesException.Status.Code.HasValue
                        && kubernetesException.Status.Code.Value > 0
                            ? (HttpStatusCode)kubernetesException.Status.Code.Value
                            : HttpStatusCode.InternalServerError,
                    Message = kubernetesException.Status.Message
                }
            );
        }

        return new KubernetesException(
            e.Message,
            new Status { Code = HttpStatusCode.InternalServerError, Message = e.Message },
            e
        );
    }

    private static bool TryDeserializeStatus(string data, out V1Status? status)
    {
        try
        {
            status = KubernetesJson.Deserialize<V1Status>(data);
            return true;
        }
        catch
        {
            status = null;
            return false;
        }
    }
}

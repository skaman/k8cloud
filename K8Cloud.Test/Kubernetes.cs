using k8s;

namespace K8Cloud.Test;

internal class Kubernetes : k8s.Kubernetes
{
    public Kubernetes(KubernetesClientConfiguration config, params DelegatingHandler[] handlers)
        : base(config, handlers) { }

    public async Task<bool> IsHealth()
    {
        var response = await SendRequest<object?>(
                "/readyz",
                HttpMethod.Get,
                null,
                null,
                CancellationToken.None
            )
            .ConfigureAwait(false);
        return response.IsSuccessStatusCode;
    }
}

using Microsoft.AspNetCore.Components;

namespace K8Cloud.Blazor.Components.Clusters;

public partial class ClusterForm
{
    public record ClusterFormData
    {
        public string ServerName { get; set; } = null!;
        public string ServerAddress { get; set; } = null!;
        public string ServerCertificateAuthorityData { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string UserCredentialsCertificateData { get; set; } = null!;
        public string UserCredentialsKeyData { get; set; } = null!;
        public string Namespace { get; set; } = null!;
    }

    [Parameter]
    public ClusterFormData Data { get; set; } = null!;
}

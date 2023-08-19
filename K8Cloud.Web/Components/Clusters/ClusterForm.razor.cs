using Microsoft.AspNetCore.Components;

namespace K8Cloud.Web.Components.Clusters;

public partial class ClusterForm
{
    public record ClusterFormData
    {
        public string ServerName { get; set; } = string.Empty;
        public string ServerAddress { get; set; } = string.Empty;
        public string ServerCertificateAuthorityData { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserCredentialsCertificateData { get; set; } = string.Empty;
        public string UserCredentialsKeyData { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
    }

    [Parameter]
    public ClusterFormData Data { get; set; } = default!;
}

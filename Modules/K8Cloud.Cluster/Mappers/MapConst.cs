namespace K8Cloud.Cluster.Mappers;

internal static class MapConst
{
    public const string Prefix = "k8cloud/";

    public const string ClusterId = $"{Prefix}clusterId";
    public const string NamespaceId = $"{Prefix}namespaceId";

    public const string CreatedAt = $"{Prefix}createdAt";
    public const string UpdatedAt = $"{Prefix}updatedAt";
    public const string Version = $"{Prefix}version";
}

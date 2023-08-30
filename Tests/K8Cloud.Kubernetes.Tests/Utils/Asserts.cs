using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Cluster.Entities;

namespace Xunit;

public partial class Assert
{
    internal static void Equal(ClusterData expected, ClusterEntity actual)
    {
        Equal(expected.ServerName, actual.ServerName);
        Equal(expected.ServerAddress, actual.ServerAddress);
        Equal(expected.ServerCertificateAuthorityData, actual.ServerCertificateAuthorityData);
        Equal(expected.UserName, actual.UserName);
        Equal(expected.UserCredentialsCertificateData, actual.UserCredentialsCertificateData);
        Equal(expected.UserCredentialsKeyData, actual.UserCredentialsKeyData);
        Equal(expected.Namespace, actual.Namespace);
    }

    internal static void Equal(ClusterEntity expected, ClusterEntity actual)
    {
        Equal(expected.Id, actual.Id);
        EqualWithMsResolution(expected.CreatedAt, actual.CreatedAt);
        EqualWithMsResolution(expected.UpdatedAt, actual.UpdatedAt);
        Equal(expected.Version, actual.Version);
        Equal(expected.ServerName, actual.ServerName);
        Equal(expected.ServerAddress, actual.ServerAddress);
        Equal(expected.ServerCertificateAuthorityData, actual.ServerCertificateAuthorityData);
        Equal(expected.UserName, actual.UserName);
        Equal(expected.UserCredentialsCertificateData, actual.UserCredentialsCertificateData);
        Equal(expected.UserCredentialsKeyData, actual.UserCredentialsKeyData);
        Equal(expected.Namespace, actual.Namespace);
    }

    internal static void Equal(ClusterEntity expected, ClusterResource actual)
    {
        Equal(expected.Id, actual.Id);
        EqualWithMsResolution(expected.CreatedAt, actual.CreatedAt);
        EqualWithMsResolution(expected.UpdatedAt, actual.UpdatedAt);
        Equal(expected.Version.ToString(), actual.Version);
        Equal(expected.ServerName, actual.ServerName);
        Equal(expected.ServerAddress, actual.ServerAddress);
        Equal(expected.ServerCertificateAuthorityData, actual.ServerCertificateAuthorityData);
        Equal(expected.UserName, actual.UserName);
        Equal(expected.UserCredentialsCertificateData, actual.UserCredentialsCertificateData);
        Equal(expected.UserCredentialsKeyData, actual.UserCredentialsKeyData);
        Equal(expected.Namespace, actual.Namespace);
    }

    internal static void Equal(NamespaceData expected, NamespaceEntity actual)
    {
        Equal(expected.Name, actual.Name);
    }

    internal static void Equal(NamespaceEntity expected, NamespaceEntity actual)
    {
        Equal(expected.Id, actual.Id);
        EqualWithMsResolution(expected.CreatedAt, actual.CreatedAt);
        EqualWithMsResolution(expected.UpdatedAt, actual.UpdatedAt);
        Equal(expected.Version, actual.Version);
        Equal(expected.Name, actual.Name);
    }

    internal static void Equal(NamespaceEntity expected, NamespaceResource actual)
    {
        Equal(expected.Id, actual.Id);
        Equal(expected.ClusterId, actual.ClusterId);
        EqualWithMsResolution(expected.CreatedAt, actual.CreatedAt);
        EqualWithMsResolution(expected.UpdatedAt, actual.UpdatedAt);
        Equal(expected.Version.ToString(), actual.Version);
        Equal(expected.Name, actual.Name);
    }

    internal static void Equal(NamespaceResource expected, NamespaceResource actual)
    {
        Equal(expected.Id, actual.Id);
        Equal(expected.ClusterId, actual.ClusterId);
        EqualWithMsResolution(expected.CreatedAt, actual.CreatedAt);
        EqualWithMsResolution(expected.UpdatedAt, actual.UpdatedAt);
        Equal(expected.Version.ToString(), actual.Version);
        Equal(expected.Name, actual.Name);
    }

    internal static void EqualWithMsResolution(DateTime expected, DateTime actual)
    {
        var expectedMilliseconds = (ulong)(expected - DateTime.UnixEpoch).TotalMilliseconds;
        var actualMilliseconds = (ulong)(actual - DateTime.UnixEpoch).TotalMilliseconds;

        Equal(expectedMilliseconds, actualMilliseconds);
    }
}

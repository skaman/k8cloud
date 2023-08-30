using AutoMapper;
using k8s;
using k8s.Models;

namespace K8Cloud.Cluster.Mappers;

internal static class MapperExtensions
{
    public static void MapFromLabels<TSrc, TDest, TType>(
        this IMemberConfigurationExpression<TSrc, TDest, TType> opt,
        string key
    ) where TSrc : IKubernetesObject<V1ObjectMeta>
    {
        opt.MapFrom(
            (src, dest) =>
            {
                src.Metadata.Labels.TryGetValue(key, out var value);
                return value;
            }
        );
    }

    public static void MapFromAnnotations<TSrc, TDest, TType>(
        this IMemberConfigurationExpression<TSrc, TDest, TType> opt,
        string key
    ) where TSrc : IKubernetesObject<V1ObjectMeta>
    {
        opt.MapFrom(
            (src, dest) =>
            {
                src.Metadata.Annotations.TryGetValue(key, out var value);
                return value;
            }
        );
    }
}

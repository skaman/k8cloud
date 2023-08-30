using AutoMapper;
using K8Cloud.Cluster.Entities;
using K8Cloud.Cluster.StateMachines.Namespace;
using K8Cloud.Contracts.Kubernetes.Data;
using k8s.Models;
using System.Globalization;

namespace K8Cloud.Cluster.Mappers;

public class ToDateTimeTypeConverter : ITypeConverter<string, DateTime>
{
    public DateTime Convert(string source, DateTime destination, ResolutionContext context)
    {
        return DateTime.ParseExact(source, "o", CultureInfo.InvariantCulture).ToUniversalTime();
    }
}

public class FromDateTimeTypeConverter : ITypeConverter<DateTime, string>
{
    public string Convert(DateTime source, string destination, ResolutionContext context)
    {
        return source.ToString("o", CultureInfo.InvariantCulture);
    }
}

/// <summary>
/// AutoMapper profile for namespace.
/// </summary>
internal class NamespaceProfile : Profile
{
    public NamespaceProfile()
    {
        // TODO: move as shared
        CreateMap<string, DateTime>()
            .ConvertUsing(new ToDateTimeTypeConverter());
        CreateMap<DateTime, string>().ConvertUsing(new FromDateTimeTypeConverter());

        CreateMap<NamespaceEntity, NamespaceData>().ReverseMap();
        CreateMap<NamespaceEntity, NamespaceResource>();

        CreateMap<NamespaceSyncState, SyncInfo>()
            .ForMember(x => x.Status, opt => opt.MapFrom(src => src.CurrentState))
            .ForMember(x => x.ErrorStatus, opt => opt.MapFrom(src => src.ErrorStatus));

        CreateMap<V1Namespace, NamespaceResource>()
            .ForMember(x => x.Id, opt => opt.MapFromLabels(MapConst.NamespaceId))
            .ForMember(x => x.ClusterId, opt => opt.MapFromLabels(MapConst.ClusterId))
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Metadata.Name))
            .ForMember(x => x.CreatedAt, opt => opt.MapFromAnnotations(MapConst.CreatedAt))
            .ForMember(x => x.UpdatedAt, opt => opt.MapFromAnnotations(MapConst.UpdatedAt))
            .ForMember(x => x.Version, opt => opt.MapFromAnnotations(MapConst.Version));

        CreateMap<NamespaceResource, V1ObjectMeta>()
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
            .AfterMap(
                (src, dest, context) =>
                {
                    dest.Labels = new Dictionary<string, string>
                    {
                        { MapConst.ClusterId, context.Mapper.Map<string>(src.ClusterId) },
                        { MapConst.NamespaceId, context.Mapper.Map<string>(src.Id) }
                    };

                    dest.Annotations = new Dictionary<string, string>
                    {
                        { MapConst.CreatedAt, context.Mapper.Map<string>(src.CreatedAt) },
                        { MapConst.UpdatedAt, context.Mapper.Map<string>(src.UpdatedAt) },
                        { MapConst.Version, src.Version }
                    };

                    dest.Finalizers = null;
                    dest.ManagedFields = null;
                    dest.OwnerReferences = null;
                }
            );

        CreateMap<NamespaceResource, V1Namespace>()
            .AfterMap(
                (src, dest, context) =>
                {
                    dest.Metadata = context.Mapper.Map<NamespaceResource, V1ObjectMeta>(src);
                    //dest.Metadata.ManagedFields = null;
                    dest.ApiVersion = V1Namespace.KubeApiVersion;
                    dest.Kind = V1Namespace.KubeKind;
                }
            );
    }
}

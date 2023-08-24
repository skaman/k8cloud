using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Entities;
using k8s.Models;

namespace K8Cloud.Kubernetes.Mappers;

/// <summary>
/// AutoMapper profile for namespace.
/// </summary>
internal class NamespaceProfile : Profile
{
    public NamespaceProfile()
    {
        CreateMap<NamespaceEntity, NamespaceData>().ReverseMap();
        CreateMap<NamespaceEntity, NamespaceResource>();

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
                }
            );

        CreateMap<NamespaceResource, V1Namespace>()
            .AfterMap(
                (src, dest, context) =>
                {
                    dest.Metadata = context.Mapper.Map<NamespaceResource, V1ObjectMeta>(src);
                }
            );
    }
}

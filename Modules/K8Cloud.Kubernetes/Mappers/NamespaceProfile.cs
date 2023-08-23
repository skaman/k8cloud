using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Entities;
using K8Cloud.Kubernetes.Services;
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

        CreateMap<NamespaceResource, V1Namespace>()
            .ForMember(x => x.Metadata.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(
                x => x.Metadata.Labels,
                opt =>
                    opt.MapFrom(
                        src =>
                            new Dictionary<string, object>
                            {
                                { MapConst.ClusterId, src.ClusterId },
                                { MapConst.NamespaceId, src.Id }
                            }
                    )
            )
            .ForMember(
                x => x.Metadata.Annotations,
                opt =>
                    opt.MapFrom(
                        src =>
                            new Dictionary<string, object>
                            {
                                { MapConst.CreatedAt, src.CreatedAt },
                                { MapConst.UpdatedAt, src.UpdatedAt },
                                { MapConst.Version, src.Version }
                            }
                    )
            );
    }
}

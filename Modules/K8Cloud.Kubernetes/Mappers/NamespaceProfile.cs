using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Entities;

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
    }
}

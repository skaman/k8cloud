using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Entities;

namespace K8Cloud.Kubernetes.Mappers;

/// <summary>
/// AutoMapper profile for cluster.
/// </summary>
internal class ClusterProfile : Profile
{
    public ClusterProfile()
    {
        CreateMap<ClusterEntity, ClusterData>().ReverseMap();
        CreateMap<ClusterEntity, ClusterResource>();
    }
}

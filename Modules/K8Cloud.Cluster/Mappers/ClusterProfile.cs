using AutoMapper;
using K8Cloud.Cluster.Entities;
using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Cluster.Mappers;

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

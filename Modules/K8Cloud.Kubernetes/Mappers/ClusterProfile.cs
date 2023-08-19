using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Contracts.Kubernetes.RequestResponse;
using K8Cloud.Kubernetes.Entities;

namespace K8Cloud.Kubernetes.Mappers;

internal class ClusterProfile : Profile
{
    public ClusterProfile()
    {
        CreateMap<AddCluster, Cluster>().IncludeMembers(s => s.Data);
        CreateMap<ClusterData, Cluster>(MemberList.None);

        CreateMap<Cluster, ClusterData>();
        CreateMap<Cluster, ClusterRecord>();

        CreateMap<Cluster, ClusterSummary>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ServerName))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.ServerAddress));
    }
}

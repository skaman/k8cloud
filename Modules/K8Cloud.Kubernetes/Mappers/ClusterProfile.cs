using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using K8Cloud.Kubernetes.Entities;

namespace K8Cloud.Kubernetes.Mappers;

internal class ClusterProfile : Profile
{
    public ClusterProfile()
    {
        CreateMap<Cluster, ClusterData>();
        CreateMap<Cluster, ClusterResource>();
    }
}

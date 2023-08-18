using AutoMapper;
using K8Cloud.Blazor.Components.Clusters;
using K8Cloud.Contracts.Kubernetes.Data;

namespace K8Cloud.Kubernetes.Mappers;

internal class ClusterProfile : Profile
{
    public ClusterProfile()
    {
        CreateMap<ClusterForm.ClusterFormData, ClusterData>();
    }
}

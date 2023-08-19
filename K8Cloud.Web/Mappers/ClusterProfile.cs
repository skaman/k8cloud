using AutoMapper;
using K8Cloud.Web.Components.Clusters;

namespace K8Cloud.Web.Mappers;

public class ClusterProfile : Profile
{
    public ClusterProfile()
    {
        CreateMap<ClusterForm.ClusterFormData, ClusterDataInput>();
        CreateMap<IEditClusterQuery_ClusterById, ClusterForm.ClusterFormData>();
    }
}

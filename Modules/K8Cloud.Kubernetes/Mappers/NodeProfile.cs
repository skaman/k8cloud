using AutoMapper;
using K8Cloud.Contracts.Kubernetes.Data;
using k8s.Models;

namespace K8Cloud.Kubernetes.Mappers;

/// <summary>
/// AutoMapper profile for node.
/// </summary>
internal class NodeProfile : Profile
{
    private const string RoleLabelPrefix = "node-role.kubernetes.io/";

    public NodeProfile()
    {
        CreateMap<V1Node, NodeInfo>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Metadata.Uid))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Metadata.Name))
            .ForMember(
                dest => dest.IPAddresses,
                opt =>
                {
                    opt.MapFrom(
                        src =>
                            src.Status.Addresses
                                .Where(x => x.Type == "InternalIP")
                                .Select(x => x.Address)
                    );
                    opt.NullSubstitute(Array.Empty<string>());
                }
            )
            .ForMember(
                dest => dest.Roles,
                opt =>
                {
                    opt.MapFrom(
                        src =>
                            src.Metadata.Labels
                                .Where(x => x.Key.StartsWith(RoleLabelPrefix))
                                .Select(x => x.Key.Substring(RoleLabelPrefix.Length))
                    );
                    opt.NullSubstitute(Array.Empty<string>());
                }
            )
            .ForMember(
                dest => dest.Conditions,
                opt =>
                {
                    opt.MapFrom(src => src.Status.Conditions);
                    opt.NullSubstitute(Array.Empty<string>());
                }
            )
            .ForMember(
                dest => dest.Architecture,
                opt => opt.MapFrom(src => src.Status.NodeInfo.Architecture)
            )
            .ForMember(dest => dest.BootID, opt => opt.MapFrom(src => src.Status.NodeInfo.BootID))
            .ForMember(
                dest => dest.ContainerRuntimeVersion,
                opt => opt.MapFrom(src => src.Status.NodeInfo.ContainerRuntimeVersion)
            )
            .ForMember(
                dest => dest.KernelVersion,
                opt => opt.MapFrom(src => src.Status.NodeInfo.KernelVersion)
            )
            .ForMember(
                dest => dest.KubeProxyVersion,
                opt => opt.MapFrom(src => src.Status.NodeInfo.KubeProxyVersion)
            )
            .ForMember(
                dest => dest.KubeletVersion,
                opt => opt.MapFrom(src => src.Status.NodeInfo.KubeletVersion)
            )
            .ForMember(
                dest => dest.MachineID,
                opt => opt.MapFrom(src => src.Status.NodeInfo.MachineID)
            )
            .ForMember(
                dest => dest.OperatingSystem,
                opt => opt.MapFrom(src => src.Status.NodeInfo.OperatingSystem)
            )
            .ForMember(dest => dest.OsImage, opt => opt.MapFrom(src => src.Status.NodeInfo.OsImage))
            .ForMember(
                dest => dest.SystemUUID,
                opt => opt.MapFrom(src => src.Status.NodeInfo.SystemUUID)
            )
            .ForMember(dest => dest.PodCIDR, opt => opt.MapFrom(src => src.Spec.PodCIDR))
            .ForMember(
                dest => dest.PodCIDRs,
                opt =>
                {
                    opt.MapFrom(src => src.Spec.PodCIDRs);
                    opt.NullSubstitute(Array.Empty<string>());
                }
            );

        CreateMap<V1NodeCondition, NodeCondition>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(
                dest => dest.IsOperative,
                opt =>
                    opt.MapFrom(
                        src => src.Type == "Ready" ? src.Status == "True" : src.Status != "True"
                    )
            );
    }
}

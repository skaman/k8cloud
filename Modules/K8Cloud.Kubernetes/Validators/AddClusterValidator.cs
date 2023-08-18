using FluentValidation;
using K8Cloud.Contracts.Kubernetes.RequestResponse;
using K8Cloud.Shared.Database;

namespace K8Cloud.Kubernetes.Validators;

internal class AddClusterValidator : AbstractValidator<AddCluster>
{
    public AddClusterValidator(ClusterDataValidator clusterDataValidator)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Data).NotNull().SetValidator(clusterDataValidator);
    }
}

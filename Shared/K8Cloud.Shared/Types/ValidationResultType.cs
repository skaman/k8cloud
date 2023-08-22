using FluentValidation.Results;
using HotChocolate.Types;

namespace K8Cloud.Shared.Types;

internal class ValidationResultType : ObjectType<ValidationResult>
{
    protected override void Configure(IObjectTypeDescriptor<ValidationResult> descriptor)
    {
        descriptor.Ignore(t => t.ToDictionary());
        descriptor.Ignore(t => t.RuleSetsExecuted);
    }
}

using FluentValidation.Results;
using HotChocolate.Types;

namespace K8Cloud.Shared.Types;

internal class ValidationFailureType : ObjectType<ValidationFailure>
{
    protected override void Configure(IObjectTypeDescriptor<ValidationFailure> descriptor)
    {
        descriptor.Ignore(t => t.FormattedMessagePlaceholderValues);
    }
}

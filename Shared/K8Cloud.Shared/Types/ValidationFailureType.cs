using FluentValidation.Results;
using HotChocolate.Types;

namespace K8Cloud.Shared.Types;

/// <summary>
/// GraphQL type for <see cref="ValidationFailure"/>.
/// </summary>
internal class ValidationFailureType : ObjectType<ValidationFailure>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<ValidationFailure> descriptor)
    {
        descriptor.Ignore(t => t.FormattedMessagePlaceholderValues);
    }
}

using FluentValidation.Results;
using HotChocolate.Types;

namespace K8Cloud.Shared.Types;

/// <summary>
/// GraphQL type for <see cref="ValidationResult"/>.
/// </summary>
internal class ValidationResultType : ObjectType<ValidationResult>
{
    /// <inheritdoc />
    protected override void Configure(IObjectTypeDescriptor<ValidationResult> descriptor)
    {
        descriptor.Ignore(t => t.ToDictionary());
        descriptor.Ignore(t => t.RuleSetsExecuted);
    }
}

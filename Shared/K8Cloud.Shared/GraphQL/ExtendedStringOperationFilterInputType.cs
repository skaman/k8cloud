using HotChocolate.Data.Filters;
using HotChocolate.Types;

namespace K8Cloud.Shared.GraphQL;

/// <summary>
/// Extended string operation filter input type.
/// </summary>
public class ExtendedStringOperationFilterInputType : StringOperationFilterInputType
{
    /// <inheritdoc />
    protected override void Configure(IFilterInputTypeDescriptor descriptor)
    {
        base.Configure(descriptor);
        descriptor.Operation(ExtendedFilterOperations.EqualsInvariant).Type<StringType>();
        descriptor.Operation(ExtendedFilterOperations.ContainsInvariant).Type<StringType>();

        descriptor.Name("StringOperationFilterInput");
    }
}

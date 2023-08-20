using HotChocolate.Data.Filters;
using HotChocolate.Types;

namespace K8Cloud.Shared.GraphQL;

public class ExtendedStringOperationFilterInputType : StringOperationFilterInputType
{
    protected override void Configure(IFilterInputTypeDescriptor descriptor)
    {
        base.Configure(descriptor);
        descriptor.Operation(ExtendedFilterOperations.EqualsInvariant).Type<StringType>();
        descriptor.Operation(ExtendedFilterOperations.ContainsInvariant).Type<StringType>();

        descriptor.Name("StringOperationFilterInput");
    }
}

using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;

namespace K8Cloud.Shared.GraphQL;

/// <summary>
/// Extended filtering convention.
/// </summary>
public class ExtendedFilteringConvention : FilterConvention
{
    /// <inheritdoc />
    protected override void Configure(IFilterConventionDescriptor descriptor)
    {
        descriptor.AddDefaults();

        descriptor.Operation(ExtendedFilterOperations.EqualsInvariant).Name("eqInvariant");
        descriptor.Operation(ExtendedFilterOperations.ContainsInvariant).Name("containsInvariant");

        descriptor.BindRuntimeType<string, ExtendedStringOperationFilterInputType>();

        descriptor.Provider(
            new QueryableFilterProvider(
                x =>
                    x.AddDefaultFieldHandlers()
                        .AddFieldHandler<QueryableStringInvariantEqualsHandler>()
                        .AddFieldHandler<QueryableStringInvariantContainsHandler>()
            )
        );
    }
}

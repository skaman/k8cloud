using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Data.Filters;
using HotChocolate.Language;
using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Types;

namespace K8Cloud.Shared.GraphQL;

public class QueryableStringInvariantEqualsHandler : QueryableStringOperationHandler
{
    private static readonly MethodInfo _toLower = typeof(string)
        .GetMethods()
        .Single(x => x.Name == nameof(string.ToLower) && x.GetParameters().Length == 0);

    public QueryableStringInvariantEqualsHandler(InputParser inputParser) : base(inputParser) { }

    protected override int Operation => ExtendedFilterOperations.EqualsInvariant;

    public override Expression HandleOperation(
        QueryableFilterContext context,
        IFilterOperationField field,
        IValueNode value,
        object? parsedValue
    )
    {
        Expression property = context.GetInstance();
        if (parsedValue is string str)
        {
            return Expression.Equal(
                Expression.Call(property, _toLower),
                Expression.Constant(str.ToLower())
            );
        }

        throw new InvalidOperationException();
    }
}

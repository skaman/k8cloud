using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Data.Filters;
using HotChocolate.Language;
using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace K8Cloud.Shared.GraphQL;

public class QueryableStringInvariantContainsHandler : QueryableStringOperationHandler
{
    private static readonly Regex sWhitespace = new Regex(@"\s+");
    private static readonly MethodInfo _ilike = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
        "ILike",
        new[] { typeof(DbFunctions), typeof(string), typeof(string) }
    )!;

    public QueryableStringInvariantContainsHandler(InputParser inputParser) : base(inputParser) { }

    protected override int Operation => ExtendedFilterOperations.ContainsInvariant;

    public override Expression HandleOperation(
        QueryableFilterContext context,
        IFilterOperationField field,
        IValueNode value,
        object? parsedValue
    )
    {
        Expression property = context.GetInstance();
        var source = Expression.Constant(EF.Functions);
        if (parsedValue is string str)
        {
            var pattern = sWhitespace.Replace(str, "%");
            pattern = $"%{pattern}%";

            return Expression.AndAlso(
                Expression.NotEqual(property, Expression.Constant(null, typeof(object))),
                Expression.Call(
                    null,
                    _ilike,
                    source,
                    property,
                    // Expression.Constant(pattern)
                    CreateParameter(pattern, property.Type)
                )
            );
        }
        throw new InvalidOperationException();
    }

    private static Expression CreateAndConvertParameter<T>(object value)
    {
        Expression<Func<T>> lambda = () => (T)value;
        return lambda.Body;
    }

    private static Expression CreateParameter(object? value, Type type) =>
        (Expression)_createAndConvert.MakeGenericMethod(type).Invoke(null, new[] { value })!;

    private static readonly MethodInfo _createAndConvert = typeof(FilterExpressionBuilder)
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
    .GetMethod(nameof(CreateAndConvertParameter), BindingFlags.NonPublic | BindingFlags.Static)!;
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
}

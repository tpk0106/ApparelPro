using System.Linq.Expressions;

namespace ApparelPro.Shared.Extensions
{
    public static class QueryableExtension 
    {
        // https://dev.to/karenpayneoregon/ef-core-order-by-string-column-name-4549
        public static IOrderedQueryable<T> OrderByColumn<T>(this IQueryable<T> source , string columnPath) =>
            source.OrderByColumnUsing(columnPath, "OrderBy");

        public static IOrderedQueryable<T> OrderByColumnDescending<T>(this IQueryable<T> source, string columnPath) =>
           source.OrderByColumnUsing(columnPath, "OrderByDescending");

        public static IOrderedQueryable<T> ThenByColumn<T>(this IQueryable<T> source, string columnPath) =>
          source.OrderByColumnUsing(columnPath, "ThenBy");

        public static IOrderedQueryable<T> ThhenByDescending<T>(this IQueryable<T> source, string columnPath) =>
          source.OrderByColumnUsing(columnPath, "ThenByDescending");

        private static IOrderedQueryable<T> OrderByColumnUsing<T>(this IQueryable<T> source, string columnPath, string method)
        {
            var parameter = Expression.Parameter(typeof(T),"Item");
            var member = columnPath.Split(".").Aggregate((Expression)parameter, Expression.PropertyOrField);
            var keySelector = Expression.Lambda(member, parameter);
            var methodCall = Expression
                .Call(typeof(Queryable), method, new[] { parameter.Type, member.Type },source.Expression,Expression.Quote(keySelector));
            return (IOrderedQueryable<T>) source.Provider.CreateQuery(methodCall);
        }
    }
}

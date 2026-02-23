using InternProject.Models.PagingModels;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace InternProject.Extensions
{
    public static class KeySetPaginationExtensions
    {
        public static async Task<PaginationKeySetModel<T, CompositeCursor<TPrimary, TSecondary>>>
            ToKeySetPaginatedListAsync<T, TPrimary, TSecondary>(
            this IQueryable<T> query,
            int pageSize,
            CompositeCursor<TPrimary, TSecondary>? cursor,
            Expression<Func<T, TPrimary>> primarySelector,
            Expression<Func<T, TSecondary>> secondarySelector,
            bool descending = true)
            where TPrimary : IComparable
            where TSecondary : IComparable
        {
            var filteredQuery = query.ApplyKeysetFilter(
                cursor,
                primarySelector,
                secondarySelector,
                descending);

            filteredQuery = descending
                ? filteredQuery.OrderByDescending(primarySelector)
                    .ThenByDescending(secondarySelector)
                : filteredQuery.OrderBy(primarySelector)
                    .ThenBy(secondarySelector);

            var items = await filteredQuery
                .Take(pageSize + 1)
                .ToListAsync();

            var hasMore = items.Count > pageSize;

            var data = hasMore
                ? [.. items.Take(pageSize)]
                : items;

            CompositeCursor<TPrimary, TSecondary>? nextCursor = default;

            if (data.Count > 0)
            {
                var last = data[^1];

                var primaryFunc = primarySelector.Compile();
                var secondaryFunc = secondarySelector.Compile();

                nextCursor = new CompositeCursor<TPrimary, TSecondary>(
                    primaryFunc(last),
                    secondaryFunc(last));
            }

            return new PaginationKeySetModel<T,
                CompositeCursor<TPrimary, TSecondary>>(
                data,
                nextCursor.GetValueOrDefault(),
                hasMore);
        }

        private static IQueryable<T> ApplyKeysetFilter<T, TPrimary, TSecondary>(
            this IQueryable<T> query,
            CompositeCursor<TPrimary, TSecondary>? cursor,
            Expression<Func<T, TPrimary>> primarySelector,
            Expression<Func<T, TSecondary>> secondarySelector,
            bool descending)
            where TPrimary : IComparable
            where TSecondary : IComparable
        {
            if (cursor == null)
                return query;

            var parameter = primarySelector.Parameters[0];

            var primaryBody = primarySelector.Body;
            var secondaryBody = new ParameterReplacer(
                secondarySelector.Parameters[0],
                parameter).Visit(secondarySelector.Body);

            var pConstant = Expression.Constant(cursor.Value.Primary);
            var sConstant = Expression.Constant(cursor.Value.Secondary);

            Expression condition;

            if (descending)
            {
                condition = Expression.OrElse(
                    Expression.LessThan(primaryBody, pConstant),
                    Expression.AndAlso(
                        Expression.Equal(primaryBody, pConstant),
                        Expression.LessThan(secondaryBody, sConstant)
                    ));
            }
            else
            {
                condition = Expression.OrElse(
                    Expression.GreaterThan(primaryBody, pConstant),
                    Expression.AndAlso(
                        Expression.Equal(primaryBody, pConstant),
                        Expression.GreaterThan(secondaryBody, sConstant)
                    ));
            }

            return query.Where(
                Expression.Lambda<Func<T, bool>>(condition, parameter));
        }

        private sealed class ParameterReplacer(
            ParameterExpression oldParam,
            ParameterExpression newParam) : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam = oldParam;
            private readonly ParameterExpression _newParam = newParam;

            protected override Expression VisitParameter(ParameterExpression node)
                => node == _oldParam ? _newParam : node;
        }
    }
}

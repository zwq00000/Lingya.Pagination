using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lingya.Pagination {
    internal static class OrderQueryableExtensions {
        /// <summary>
        /// 根据 排序字段名称 生成 新的查询
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="fieldNames">sort field names split by ','</param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static IQueryable<TSource> OrderByFields<TSource>(this IQueryable<TSource> source, string fieldNames,
            bool desc) {
            if (String.IsNullOrEmpty(fieldNames)) {
                return source;
            }

            if (fieldNames.Contains(',')) {
                var fields = fieldNames.Split(',');
                var expresses = source.GenOrderByExpression(desc, fields);
                return source.Provider.CreateQuery<TSource>(expresses);

            } else {
                return source.OrderByField(fieldNames, desc);
            }
        }

        private static IQueryable<TSource> OrderByField<TSource>(this IQueryable<TSource> source, string fieldName,
            bool desc) {
            if (String.IsNullOrEmpty(fieldName)) {
                return source;
            }

            var property = source.ElementType.GetProperty(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property == null) {
                return source;
            }

            var param = Expression.Parameter(typeof(TSource), "i");
            var propertyAccess = Expression.MakeMemberAccess(param, property);
            var sortExpression = Expression.Lambda(propertyAccess, param);

            var sortMethod = GetSortMethodName(desc);;

            var result = Expression.Call(
                typeof(Queryable),
                sortMethod,
                new Type[] { source.ElementType, property.PropertyType },
                source.Expression,
                Expression.Quote(sortExpression));

            return source.Provider.CreateQuery<TSource>(result);
        }

        private static MethodCallExpression GetOrderByExpression<TSource>(this IQueryable<TSource> source,
            string fieldName, bool desc) {
            if (String.IsNullOrEmpty(fieldName)) {
                return null;
            }

            var type = typeof(TSource);
            var property = type.GetProperty(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property == null) {
                return null;
            }

            var param = Expression.Parameter(typeof(TSource), "i");
            var propertyAccess = Expression.MakeMemberAccess(param, property);
            var sortExpression = Expression.Lambda(propertyAccess, param);

            var sortMethod = GetSortMethodName(desc);

            var result = Expression.Call(
                typeof(Queryable),
                sortMethod,
                new Type[] { type, property.PropertyType },
                source.Expression,
                Expression.Quote(sortExpression));
            return result;
        }


        private static Expression GenOrderByExpression<TSource>(this IQueryable<TSource> source, bool desc,
            params string[] fields) {
            if (fields.Length == 0) {
                return source.Expression;
            }

            Expression queryExpr = source.Expression;
            string methodOrderByAsc = nameof(Queryable.OrderBy);
            string methodOrderByDesc = nameof(Queryable.OrderByDescending);

            foreach (var field in fields) {
                var exp = source.ElementType.GenMemberAccessExpression(field);
                var property = source.ElementType.GetProperty(field,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property == null) {
                    continue;
                }

                queryExpr = Expression.Call(
                    typeof(Queryable), desc ? methodOrderByDesc : methodOrderByAsc,
                    new Type[] { source.ElementType, property.PropertyType },
                    queryExpr, Expression.Quote(exp));

                methodOrderByAsc = nameof(Queryable.ThenBy);
                methodOrderByDesc = nameof(Queryable.ThenByDescending);
            }

            return queryExpr;
        }

        /// <summary>
        /// 生成属性访问 Lambda表达式
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static LambdaExpression GenMemberAccessExpression(this Type elementType, string propertyName) {
            var property = elementType.GetProperty(propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property == null) {
                return null;
            }

            var param = Expression.Parameter(elementType, "i");
            var propertyAccess = Expression.MakeMemberAccess(param, property);
            var sortExpression = Expression.Lambda(propertyAccess, param);
            return sortExpression;
        }

        private static string GetSortMethodName(bool descending){
            return descending ? nameof(Queryable.OrderByDescending):nameof(Queryable.OrderBy);
        }
    }
}
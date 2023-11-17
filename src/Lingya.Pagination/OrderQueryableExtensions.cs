using System;
using System.Collections.Generic;
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

            var fields = fieldNames.Split(',');
            var expresses = source.MakeOrderByExpression(desc, fields);
            if (expresses == source.Expression) {
                return source;
            }
            return source.Provider.CreateQuery<TSource>(expresses);
        }

        /// <summary>
        /// Make <see cref="Queryable.OrderBy{TSource, TKey}(IQueryable{TSource}, Expression{Func{TSource, TKey}})"/>
        /// <see cref="Queryable.OrderByDescending{TSource, TKey}(IQueryable{TSource}, Expression{Func{TSource, TKey}})"/> expression
        /// for fields
        /// </summary>
        /// <param name="source"></param>
        /// <param name="desc"></param>
        /// <param name="fields"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        private static Expression MakeOrderByExpression<TSource>(this IQueryable<TSource> source, bool desc,
            params string[] fields) {
            if (fields.Length == 0) {
                return source.Expression;
            }

            Expression queryExpr = source.Expression;
            string methodOrderByAsc = nameof(Queryable.OrderBy);
            string methodOrderByDesc = nameof(Queryable.OrderByDescending);

            foreach (var field in fields) {
                var exp = source.ElementType.GenMemberAccessExpression(field);
                if(exp==null){
                    continue;
                }
                var propertyType = exp.Body.Type;
                queryExpr = Expression.Call(
                    typeof(Queryable), desc ? methodOrderByDesc : methodOrderByAsc,
                    new Type[] { source.ElementType, propertyType },
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
            var param = Expression.Parameter(elementType, "i");
            var propertyAccess = BuildMemberAccess(param, elementType, propertyName.Split('.'));
            if (propertyAccess == null) {
                return null;
            }
            var sortExpression = Expression.Lambda(propertyAccess, param);
            return sortExpression;
        }

        private static MemberExpression BuildMemberAccess(Expression expression, Type ownerType, IEnumerable<string> propertyNames) {
            if (!propertyNames.Any()) {
                return null;
            }
            var firstMemberName = propertyNames.First();
            var memberInfo = ownerType.GetProperty(firstMemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (memberInfo == null) {
                return null;
            }
            var exp = Expression.MakeMemberAccess(expression, memberInfo);
            var nextFields = propertyNames.Skip(1);
            if (nextFields.Any()) {
                return BuildMemberAccess(exp, memberInfo.PropertyType, nextFields);
            } else {
                return exp;
            }
        }

        private static string GetSortMethodName(bool descending) {
            return descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
        }
    }
}
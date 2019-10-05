using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lingya.Pagination
{
    public static class FilterExtensions
    {

        public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> sources, PageParameter parameter) {
            if (parameter == null || !parameter.HasSearchKey()) {
                return sources;
            }
            var fields = typeof(TSource).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType == typeof(string));
            var whereExpression = sources.GenFilterExpression(parameter.SearchKey, fields.ToArray());
            return sources.Where((Expression<Func<TSource,bool>>)whereExpression);
        }

        private static Expression ToContains(Expression instance, string searchStr) {
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
            return LambdaExpression.Call(instance, containsMethod, Expression.Constant(searchStr));
        }

        private static Expression ToStartWith(Expression instance, string searchStr) {
            var containsMethod = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
            return LambdaExpression.Call(instance, containsMethod, Expression.Constant(searchStr));
        }

        public static Expression GenFilterExpression<TSource>(this IQueryable<TSource> source,
            string searchStr,
            params string[] fields) {
            if (fields.Length == 0) {
                return source.Expression;
            }

            var eParam = Expression.Parameter(typeof(TSource), "e");

            var exps = eParam.MakeMemberAccess(fields).Select(m => ToContains(m, searchStr));
            if (exps.Any()) {
                var result = exps.First();
                foreach (var exp in exps.Skip(1)) {
                    result = Expression.Or(result, exp);
                }
                return result;
            }

            return source.Expression;
        }

        public static Expression GenFilterExpression<TSource>(this IQueryable<TSource> source,
            string searchStr,
            params PropertyInfo[] fields) {
            if (fields.Length == 0) {
                return source.Expression;
            }

            var eParam = Expression.Parameter(typeof(TSource), "e");

            var exps = fields.Select(f => Expression.MakeMemberAccess(eParam, f))
                .Select(m => ToContains(m, searchStr));
            if (exps.Any()) {
                var result = exps.First();
                foreach (var exp in exps.Skip(1)) {
                    result = Expression.Or(result, exp);
                }
                return Expression.Lambda(result,eParam);
            }

            return source.Expression;
        }

        private static IEnumerable<MemberExpression> MakeMemberAccess(this ParameterExpression parameter, params string[] properties) {
            foreach (var propertyName in properties) {
                var property = parameter.Type.GetProperty(propertyName,
                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property == null) {
                    continue;
                }
                yield return Expression.MakeMemberAccess(parameter,
                                                   property);
            }
        }

        private static IEnumerable<MemberExpression> MakeMemberAccess(this ParameterExpression parameter,
            params PropertyInfo[] properties) {
            foreach (var property in properties) {
                if (property == null) {
                    continue;
                }
                yield return Expression.MakeMemberAccess(parameter,
                                                   property);
            }
        }


        /// <summary>
        /// 生成属性访问 Lambda表达式
        /// </summary>
        /// <param name="elementType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static LambdaExpression MakeMemberAccess(this Type elementType, string propertyName) {
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
    }

    public enum SearchMethod
    {
        /// <summary>
        /// 包含
        /// </summary>
        Contains,
        /// <summary>
        /// 开始字符相同
        /// </summary>
        FirstWith,
        /// <summary>
        /// 结束字符相同
        /// </summary>
        EndWith
    }
}
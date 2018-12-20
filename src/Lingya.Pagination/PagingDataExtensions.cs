using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lingya.Pagination {
    public static class PagingExtensions
    {
        /// <summary>
        /// 默认分页大小
        /// </summary>
        private const int DEFAULT_PAGE_SIZE = 20;

        public static bool HasSearchKey(this PageParameter parameter)
        {
            return !string.IsNullOrEmpty(parameter?.SearchKey);
        }


        /// <summary>
        /// 生成分页结果
        /// </summary>
        /// <remarks>
        /// 使用 SqlServerDbContextOptionsBuilder.UseRowNumberForPaging 时,使用 SortBy 字段会发生错误
        /// 
        ///  Use a ROW_NUMBER() in queries instead of OFFSET/FETCH. This method is backwards-compatible to SQL Server 2005.
        /// </remarks>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static async Task<PageResult<TResult>> PagingAsync<TSource, TResult>(this IQueryable<TSource> source,
            PageParameter parameter, Expression<Func<TSource, TResult>> selector)
        {
            if (parameter == null)
            {
                parameter = new PageParameter();
            }

            var size = parameter.PageSize;
            var pNumber = parameter.Page < 1 ? 1 : parameter.Page;
            var paging = await source.CreatePagingAsync(size, pNumber);

            if (!string.IsNullOrEmpty(parameter.SortBy))
            {
                source = source.OrderByFields(parameter.SortBy, parameter.Descending);
                //先查询再选择
                var data = (await source.ToPage(paging)).Select(selector.Compile());
                return new PageResult<TResult>(paging, data);
            } else
            {
                //没有 排序字段,优化查询
                return new PageResult<TResult>(paging, source.Select(selector));
            }
        }

        public static async Task<PageResult<TSource>> PagingAsync<TSource>(this IQueryable<TSource> source,
            PageParameter parameter)
        {
            if (parameter == null)
            {
                parameter = new PageParameter();
            }

            var size = parameter.PageSize;
            var pNumber = parameter.Page < 1 ? 1 : parameter.Page;
            if (!string.IsNullOrEmpty(parameter.SortBy))
            {
                source = source.OrderByFields(parameter.SortBy, parameter.Descending);
            }

            var paging = await source.CreatePagingAsync(size, pNumber);
            return new PageResult<TSource>(paging, source);
        }

        /// <summary>
        /// 数据分页异步方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public static async Task<PageResult<T>> PagingAsync<T>(this IQueryable<T> source, int? pageSize,
            int? pageNumber)
        {
            var psize = pageSize ?? DEFAULT_PAGE_SIZE;
            var pNumber = pageNumber ?? 1;
            var paging = await source.CreatePagingAsync(psize, pNumber);
            return new PageResult<T>(paging, source);
        }

        private static async Task<IEnumerable<T>> ToPage<T>(this IQueryable<T> source, Paging page)
        {
            return await source.Skip(page.Skip).Take(page.PageSize).ToArrayAsync();
        }

        private static async Task<Paging> CreatePagingAsync<T>(this IQueryable<T> source, int pageSize, int pageNumber)
        {
            var count = await source.CountAsync();
            return new Paging(count, pageSize, pageNumber);
        }
    }

    internal static class OrderQueryableExtensions
    {
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

            var type = typeof(TSource);
            var property = type.GetProperty(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property == null) {
                return source;
            }

            var param = Expression.Parameter(typeof(TSource), "i");
            var propertyAccess = Expression.MakeMemberAccess(param, property);
            var sortExpression = Expression.Lambda(propertyAccess, param);

            var cmd = desc ? "OrderByDescending" : "OrderBy";

            var result = Expression.Call(
                typeof(Queryable),
                cmd,
                new Type[] { type, property.PropertyType },
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

            var ascending = desc ? "OrderByDescending" : "OrderBy";

            var result = Expression.Call(
                typeof(Queryable),
                @ascending,
                new Type[] { type, property.PropertyType },
                source.Expression,
                Expression.Quote(sortExpression));
            return result;
        }


        private static Expression GenOrderByExpression<TSource>(this IQueryable<TSource> source, bool desc,
            params string[] fields) {
            if (fields.Length == 0){
                return source.Expression;
            }

            Expression queryExpr = source.Expression;
            string methodAsc = "OrderBy";
            string methodDesc = "OrderByDescending";

            foreach (var field in fields) {
                var exp = source.ElementType.GenMemberAccessExpression(field);
                var property = source.ElementType.GetProperty(field,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property == null) {
                    continue;
                }

                queryExpr = Expression.Call(
                    typeof(Queryable), desc ? methodDesc : methodAsc,
                    new Type[] { source.ElementType, property.PropertyType },
                    queryExpr, Expression.Quote(exp));

                methodAsc = "ThenBy";
                methodDesc = "ThenByDescending";
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
    }
}
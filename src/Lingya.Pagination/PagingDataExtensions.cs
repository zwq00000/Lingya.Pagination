using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lingya.Pagination {
    public static class PagingExtensions {
        /// <summary>
        /// 默认分页大小
        /// </summary>
        private const int DEFAULT_PAGE_SIZE = 20;

        public static bool HasSearchKey(this PageParamete paramete) {
            return !string.IsNullOrEmpty(paramete?.SearchKey);
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
        /// <param name="paramete"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static async Task<PageResult<TResult>> PagingAsync<TSource,TResult>(this IQueryable<TSource> source, PageParamete paramete,Expression<Func<TSource, TResult>> selector) {
            if (paramete == null) {
                paramete = new PageParamete();
            }

            var psize = paramete.PageSize;
            var pNumber = paramete.Page < 1 ? 1 : paramete.Page;
            var paging = await source.CreatePagingAsync(psize, pNumber);

            if (!string.IsNullOrEmpty(paramete.SortBy)) {
                source = source.OrderByField(paramete.SortBy, paramete.Descending);
                //先查询再选择
                var data = (await source.ToPage(paging)).Select(selector.Compile());
                return new PageResult<TResult>(paging, data);
            } else {
                //没有 排序字段,优化查询
                return new PageResult<TResult>(paging, source.Select(selector));
            }
        }

        public static async Task<PageResult<TSource>> PagingAsync<TSource>(this IQueryable<TSource> source, PageParamete paramete) {
            if (paramete == null) {
                paramete = new PageParamete();
            }

            var psize = paramete.PageSize;
            var pNumber = paramete.Page<1 ?1:paramete.Page;
            if (!string.IsNullOrEmpty(paramete.SortBy)) {
                source = source.OrderByField(paramete.SortBy, paramete.Descending);
            }

            var paging = await source.CreatePagingAsync(psize, pNumber);
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
        public static async Task<PageResult<T>> PagingAsync<T>(this IQueryable<T> source, int? pageSize, int? pageNumber) {
            var psize = pageSize ?? DEFAULT_PAGE_SIZE;
            var pNumber = pageNumber ?? 1;
            var paging = await source.CreatePagingAsync(psize, pNumber);
            return new PageResult<T>(paging, source);
        }

        private static async Task<IEnumerable<T>> ToPage<T>(this IQueryable<T> source, Paging page) {
            return await source.Skip(page.Skip).Take(page.PageSize).ToArrayAsync();
        }

        private static async Task<Paging> CreatePagingAsync<T>(this IQueryable<T> source, int pageSize, int pageNumber) {
            var count = await source.CountAsync();
            return new Paging(count, pageSize, pageNumber);
        }

        public static IQueryable<TSource> OrderByField<TSource>(this IQueryable<TSource> source, string fieldName, bool desc) {
            if (string.IsNullOrEmpty(fieldName)) {
                return source;
            }

            var type = typeof(TSource);
            var property = type.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property == null) {
                return source;
            }
            var param = Expression.Parameter(typeof(TSource), "i");
            var propertyAcess = Expression.MakeMemberAccess(param, property);
            var sortExpression = Expression.Lambda(propertyAcess, param);

            var cmd = desc ? "OrderByDescending" : "OrderBy";

            var result = Expression.Call(
                typeof(Queryable),
                cmd,
                new Type[] { type, property.PropertyType },
                source.Expression,
                Expression.Quote(sortExpression));

            return source.Provider.CreateQuery<TSource>(result);
        }
    }
}
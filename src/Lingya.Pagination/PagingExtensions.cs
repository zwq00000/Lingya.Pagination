using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Lingya.Pagination {
    /// <summary>
    /// IQueryable{T} 分页扩展方法,支持普通集合的分页功能
    /// </summary>
    public static class PagingExtensions {
        /// <summary>
        /// 默认分页大小
        /// </summary>
        private const int DEFAULT_PAGE_SIZE = 20;

        public static PageResult<TResult> ToPaging<TSource, TResult>(this IEnumerable<TSource> source,
            PageParameter parameter, Expression<Func<TSource, TResult>> selector) {
            return source.AsQueryable().ToPaging(parameter, selector);
        }

        public static PageResult<TSource> ToPaging<TSource>(this IEnumerable<TSource> source, PageParameter parameter) {
            return source.AsQueryable().ToPaging(parameter);
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
            public static PageResult<TResult> ToPaging<TSource, TResult>(this IQueryable<TSource> source,
            PageParameter parameter, Expression<Func<TSource, TResult>> selector) {
            if (parameter == null) {
                parameter = new PageParameter();
            }

            var size = parameter.PageSize;
            var pNumber = parameter.Page < 1 ? 1 : parameter.Page;
            var paging = source.CreatePaging(size, pNumber);

            if (!string.IsNullOrEmpty(parameter.SortBy)) {
                source = source.OrderByFields(parameter.SortBy, parameter.Descending);
                //先查询再选择
                var data = source.ToPage(paging).Select(selector.Compile());
                return new PageResult<TResult>(paging, data);
            } else {
                //没有 排序字段,优化查询
                return new PageResult<TResult>(paging, source.Select(selector));
            }
        }

        public static PageResult<TSource> ToPaging<TSource>(this IQueryable<TSource> source, PageParameter parameter) {
            if (parameter == null) {
                parameter = new PageParameter();
            }

            var size = parameter.PageSize;
            var pNumber = parameter.Page < 1 ? 1 : parameter.Page;
            if (!string.IsNullOrEmpty(parameter.SortBy)) {
                source = source.OrderByFields(parameter.SortBy, parameter.Descending);
            }

            var paging = source.CreatePaging(size, pNumber);
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
        public static PageResult<T> ToPaging<T>(this IQueryable<T> source, int? pageSize,
            int? pageNumber) {
            var psize = pageSize ?? DEFAULT_PAGE_SIZE;
            var pNumber = pageNumber ?? 1;
            var paging = source.CreatePaging(psize, pNumber);
            return new PageResult<T>(paging, source);
        }

        private static IEnumerable<T> ToPage<T>(this IQueryable<T> source, Paging page) {
            return source.Skip(page.Skip).Take(page.PageSize).ToArray();
        }

        private static Paging CreatePaging<T>(this IQueryable<T> source, int pageSize, int pageNumber) {
            var count = source.Count();
            return new Paging(count, pageSize, pageNumber);
        }
    }
}
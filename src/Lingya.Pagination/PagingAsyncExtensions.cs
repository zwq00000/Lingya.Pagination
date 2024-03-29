﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lingya.Pagination
{
    /// <summary>
    /// IQueryable{T} 异步分页扩展方法, IQueryable 需要支持 EntityFramework.Core
    /// </summary>
    public static class PagingAsyncExtensions {
        /// <summary>
        /// 默认分页大小
        /// </summary>
        private const int DEFAULT_PAGE_SIZE = 20;

        /// <summary>
        /// 默认页码
        /// </summary>
        private const int DEFAULT_PAGE_NUMBER = 1;

        public static bool HasSearchKey(this PageParameter parameter) {
            return !string.IsNullOrEmpty(parameter?.SearchKey);
        }


        /// <summary>
        /// 生成分页结果
        /// </summary>
        /// <remarks>
        /// 使用 SqlServerDbContextOptionsBuilder.UseRowNumberForPaging 时,使用 SortBy 字段会发生错误
        ///  Use a ROW_NUMBER() in queries instead of OFFSET/FETCH. This method is backwards-compatible to SQL Server 2005.
        /// </remarks>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <param name="selector">输出结果选择器,<see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, int, TResult})"/></param>
        /// <returns></returns>
        public static async Task<PageResult<TResult>> ToPagingAsync<TSource, TResult>(this IQueryable<TSource> source,
            PageParameter parameter, Expression<Func<TSource, TResult>> selector) {
            if (parameter == null) {
                parameter = new PageParameter();
            }

            var size = parameter.PageSize;
            var pNumber = parameter.Page < 1 ? 1 : parameter.Page;
            var paging = await source.CreatePagingAsync(size, pNumber);

            if (!string.IsNullOrEmpty(parameter.SortBy)) {
                source = source.OrderByFields(parameter.SortBy, parameter.Descending);
                // //先查询再选择
                // var data = (await source.ToPageAsync(paging)).Select(selector.Compile());
                // return new PageResult<TResult>(paging, data);
            } 
            return new PageResult<TResult>(paging, source.Select(selector));
        }

        public static async Task<PageResult<TSource>> ToPagingAsync<TSource>(this IQueryable<TSource> source,
            PageParameter parameter) {
            if (parameter == null) {
                parameter = new PageParameter();
            }

            if (!string.IsNullOrEmpty(parameter.SortBy)) {
                source = source.OrderByFields(parameter.SortBy, parameter.Descending);
            }

            var paging = await source.CreatePagingAsync(parameter.PageSize, parameter.Page);
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
        public static async Task<PageResult<T>> ToPagingAsync<T>(this IQueryable<T> source, int? pageSize,
            int? pageNumber) {
            var paging = await source.CreatePagingAsync(pageSize, pageNumber);
            return new PageResult<T>(paging, source);
        }

        private static async Task<Paging> CreatePagingAsync<T>(this IQueryable<T> source, int? pageSize, int? pageNumber) {
            var count = await source.CountAsync();
            return new Paging(count, pageSize??DEFAULT_PAGE_SIZE, pageNumber??DEFAULT_PAGE_NUMBER);
        }
    }
}
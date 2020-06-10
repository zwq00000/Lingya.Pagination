using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lingya.Pagination {
    public interface IPagingQueryBuilder<TSource> {

        /// <summary>
        /// 增加 使用 <see cref="String.Contains(string)" /> 搜索的过滤属性
        /// 属性之间使用 逻辑或 连接
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        IPagingQueryBuilder<TSource> Contains (Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others);

        /// <summary>
        /// 增加 使用 <see cref="String.StartsWith(string)" /> 搜索的过滤属性
        /// 属性之间使用 逻辑或 连接
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        IPagingQueryBuilder<TSource> StartsWith (Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others);

        /// <summary>
        /// 增加 使用 <see cref="String.EndsWith(string)" /> 搜索的过滤属性
        /// 属性之间使用 逻辑或 连接
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        IPagingQueryBuilder<TSource> EndsWith (Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others);

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <returns></returns>
        PageResult<TSource> ToPaging ();

        /// <summary>
        /// 异步获取分页结果
        /// </summary>
        /// <returns></returns>
        Task<PageResult<TSource>> ToPagingAsync ();

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="selector">转换选择器
        /// <see cref="Queryable.Select{TSource, TResult}(IQueryable{TSource}, Expression{Func{TSource, TResult}})"/>
        /// </param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        PageResult<TResult> ToPaging<TResult> (Expression<Func<TSource, TResult>> selector);

        /// <summary>
        /// 异步获取分页结果
        /// </summary>
        /// <param name="selector">转换选择器
        /// <see cref="Queryable.Select{TSource, TResult}(IQueryable{TSource}, Expression{Func{TSource, TResult}})"/>
        /// </param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        Task<PageResult<TResult>> ToPagingAsync<TResult> (Expression<Func<TSource, TResult>> selector);
    }

}
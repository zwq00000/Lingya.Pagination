using System;
using System.Linq.Expressions;

namespace Lingya.Pagination {
    /// <summary>
    /// 关键字搜索
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    public interface ISearchOptions<TSource> {
        /// <summary>
        /// 增加 使用 <see cref="string.Contains(string)" /> 搜索的过滤属性
        /// 属性之间使用 逻辑或 连接
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        ISearchOptions<TSource> ContainsFor(Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others);

        /// <summary>
        /// 增加 使用 <see cref="string.StartsWith(string)" /> 搜索的过滤属性
        /// 属性之间使用 逻辑或 连接
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        ISearchOptions<TSource> StartsFor(Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others);

        /// <summary>
        /// 增加 使用 <see cref="string.EndsWith(string)" /> 搜索的过滤属性
        /// 属性之间使用 逻辑或 连接
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        ISearchOptions<TSource> EndsFor(Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others);

        /// <summary>
        /// 构建 表达式
        /// </summary>
        /// <returns></returns>
        Expression<Func<TSource, bool>> BuildExpression();
    }

}
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lingya.Pagination {
    /// <summary>
    /// PagingQueryBuilder 扩展方法
    /// </summary>
    public static class PagingQueryBuilderExtensions {

        /// <summary>
        /// 创建分页查询构建器
        /// </summary>
        /// <param name="source">query source</param>
        /// <param name="parameter">paging parameter</param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns>IPagingQueryBuilder</returns>
        public static IPagingQueryBuilder<TSource> PagingBuilder<TSource> (this IQueryable<TSource> source,
            PageParameter parameter) {

            if (parameter == null) {
                parameter = new PageParameter ();
            }

            return new PagingQueryBuilder<TSource> (source, parameter);
        }

        /// <summary>
        /// 构建搜索表达式
        /// </summary>
        /// <param name="builder">分页查询构造器</param>
        /// <param name="seachBuilder">关键字搜索配置</param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static IPagingQueryBuilder<TSource> Search<TSource> (this IPagingQueryBuilder<TSource> builder, Action<ISearchOptions<TSource>> seachBuilder) {
            var options = new SearchOptions<TSource> (builder.Parameter);
            seachBuilder (options);
            builder.ApplyFilter (options.BuildExpression ());
            return builder;
        }

        #region  ToPage
        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="builder">分页构造器</param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static PageResult<TSource> ToPaging<TSource> (this IPagingQueryBuilder<TSource> builder) {
            return builder.Query.ToPaging (builder.Parameter);
        }

        /// <summary>
        /// 获取分页结果 的异步方法
        /// </summary>
        /// <param name="builder"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static async Task<PageResult<TSource>> ToPagingAsync<TSource> (this IPagingQueryBuilder<TSource> builder) {
            return await builder.Query.ToPagingAsync (builder.Parameter);
        }

        /// <summary>
        /// 获取分页结果
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="selector">转换选择器
        /// <see cref="Queryable.Select{TSource, TResult}(IQueryable{TSource}, Expression{Func{TSource, TResult}})"/>
        /// </param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static PageResult<TResult> ToPaging<TSource, TResult> (this IPagingQueryBuilder<TSource> builder, Expression<Func<TSource, TResult>> selector) {
            return builder.Query.ToPaging (builder.Parameter, selector);
        }

        /// <summary>
        /// 异步获取分页结果
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="selector">转换选择器
        /// <see cref="Queryable.Select{TSource, TResult}(IQueryable{TSource}, Expression{Func{TSource, TResult}})"/>
        /// </param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static async Task<PageResult<TResult>> ToPagingAsync<TSource, TResult> (this IPagingQueryBuilder<TSource> builder, Expression<Func<TSource, TResult>> selector) {
            return await builder.Query.ToPagingAsync (builder.Parameter, selector);
        }

        #endregion

        #region  Aggregate

        /// <summary>
        /// 返回带有 聚合数据的分页结果
        /// </summary>
        /// <param name="builder">分页查询构造器</param>
        /// <param name="aggExpression">聚合表达式</param>
        /// <typeparam name="TSource">原类型</typeparam>
        /// <typeparam name="TAggregate">聚合类型</typeparam>

        public static PageResult<TSource, TAggregate> WithAggregate<TSource, TAggregate> (this IPagingQueryBuilder<TSource> builder, Expression<Func<IGrouping<bool, TSource>, TAggregate>> aggExpression) {
            var aggregate = builder.Query.GroupBy (t => true).Select (aggExpression).Single ();
            var result = builder.ToPaging ();
            return new PageResult<TSource, TAggregate> (result, aggregate);
        }

        /// <summary>
        /// 返回带有 聚合数据的分页结果 的异步方法
        /// </summary>
        /// <param name="builder">分页查询构造器</param>
        /// <param name="aggExpression">聚合表达式</param>
        /// <typeparam name="TSource">原类型</typeparam>
        /// <typeparam name="TAggregate">聚合类型</typeparam>
        /// <returns></returns>
        public static async Task<PageResult<TSource, TAggregate>> WithAggregateAsync<TSource, TAggregate> (this IPagingQueryBuilder<TSource> builder, Expression<Func<IGrouping<bool, TSource>, TAggregate>> aggExpression) {
            var aggregate = await builder.Query.GroupBy (t => true).Select (aggExpression).SingleAsync ();
            var result = await builder.ToPagingAsync ();
            return new PageResult<TSource, TAggregate> (result, aggregate);
        }

        /// <summary>
        /// 返回带有 聚合数据的分页结果
        /// 支持 自定义复杂映射的聚合方法
        /// </summary>
        /// <param name="builder">分页查询构造器</param>
        /// <param name="projection">自定义映射</param>
        /// <param name="aggExpression">聚合表达式</param>
        /// <typeparam name="TSource">原类型</typeparam>
        /// <typeparam name="TAggregate">聚合类型</typeparam>
        public static PageResult<TSource, TAggregate> WithAggregate<TSource, TAggregate> (this IPagingQueryBuilder<TSource> builder,
            Expression<Func<TSource, TAggregate>> projection,
            Expression<Func<IGrouping<bool, TAggregate>, TAggregate>> aggExpression) {
            var aggregate = builder.Query.Select (projection)
                .GroupBy (t => true)
                .Select (aggExpression)
                .Single ();
            var result = builder.ToPaging ();
            return new PageResult<TSource, TAggregate> (result, aggregate);
        }

        /// <summary>
        /// 返回带有 聚合数据的分页结果的异步方法
        /// 支持 自定义复杂映射的聚合方法
        /// </summary>
        /// <param name="builder">分页查询构造器</param>
        /// <param name="projection">自定义映射</param>
        /// <param name="aggExpression">聚合表达式</param>
        /// <typeparam name="TSource">原类型</typeparam>
        /// <typeparam name="TAggregate">聚合类型</typeparam>
        /// <returns></returns>
        public static async Task<PageResult<TSource, TAggregate>> WithAggregateAsync<TSource, TAggregate> (this IPagingQueryBuilder<TSource> builder,
            Expression<Func<TSource, TAggregate>> projection,
            Expression<Func<IGrouping<bool, TAggregate>, TAggregate>> aggExpression) {
            var aggregate = await builder.Query.Select (projection)
                .GroupBy (t => true)
                .Select (aggExpression)
                .SingleAsync ();
            var result = await builder.ToPagingAsync ();
            return new PageResult<TSource, TAggregate> (result, aggregate);
        }
        #endregion
    }

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
        ISearchOptions<TSource> ContainsFor (Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others);

        /// <summary>
        /// 增加 使用 <see cref="string.StartsWith(string)" /> 搜索的过滤属性
        /// 属性之间使用 逻辑或 连接
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        ISearchOptions<TSource> StartsFor (Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others);

        /// <summary>
        /// 增加 使用 <see cref="string.EndsWith(string)" /> 搜索的过滤属性
        /// 属性之间使用 逻辑或 连接
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        ISearchOptions<TSource> EndsFor (Expression<Func<TSource, string>> expression, params Expression<Func<TSource, string>>[] others);

        /// <summary>
        /// 构建 表达式
        /// </summary>
        /// <returns></returns>
        Expression<Func<TSource, bool>> BuildExpression ();
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lingya.Pagination {
    public static class PagingBuilderExtensions {

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
        /// <param name="builder"></param>
        /// <param name="actionBuilder"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static IPagingQueryBuilder<TSource> Search<TSource> (this IPagingQueryBuilder<TSource> builder, Action<ISearchOptions<TSource>> actionBuilder) {
            var options = new SearchOptions<TSource> (builder.Parameter);
            actionBuilder (options);
            builder.Filter (options.BuildExpression ());
            return builder;
        }

        /// <summary>
        /// 构造 聚合结果
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="aggExpression"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TAggregate"></typeparam>
        public static PageResult<TSource, TAggregate> WithAggregate<TSource, TAggregate> (this IPagingQueryBuilder<TSource> builder, Expression<Func<IGrouping<bool, TSource>, TAggregate>> aggExpression) {
            var aggregate = builder.Query.GroupBy (t => true).Select (aggExpression).Single ();
            var result = builder.ToPaging ();
            return new PageResult<TSource, TAggregate> (result, aggregate);
        }

        /// <summary>
        /// 增加 聚合结果
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="aggExpression"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TAggregate"></typeparam>
        /// <returns></returns>
        public static async Task<PageResult<TSource, TAggregate>> WithAggregateAsync<TSource, TAggregate> (this IPagingQueryBuilder<TSource> builder, Expression<Func<IGrouping<bool, TSource>, TAggregate>> aggExpression) {
            var aggregate = await builder.Query.GroupBy (t => true).Select (aggExpression).SingleAsync ();
            var result = await builder.ToPagingAsync ();
            return new PageResult<TSource, TAggregate> (result, aggregate);
        }
    }

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

    internal class SearchOptions<TSource> : ISearchOptions<TSource> {
        private readonly PageParameter parameter;
        private readonly IList<Expression<Func<TSource, bool>>> expressions;

        public SearchOptions (PageParameter parameter) {
            this.parameter = parameter;
            expressions = new List<Expression<Func<TSource, bool>>> ();
        }

        /// <summary>
        /// 构造 过滤表达式
        /// </summary>
        /// <returns></returns>
        public Expression<Func<TSource, bool>> BuildExpression () {
            if (!expressions.Any ()) {
                return s => true;
            }
            if (expressions.Count == 1) {
                return expressions.First ();
            }
            var first = expressions.First ();
            var logics = first.Or (expressions.Skip (1).ToArray ());
            var parameter = PagingQueryExtensions.GetParameterExpression (first);
            logics = new PagingQueryExtensions.ParameterReplacer (parameter).Visit (logics);
            return Expression.Lambda<Func<TSource, bool>> (logics, false, parameter);
        }

        public ISearchOptions<TSource> ContainsFor (Expression<Func<TSource, string>> member, params Expression<Func<TSource, string>>[] others) {
            if (!parameter.HasSearchKey ()) {
                return this;
            }

            var exp = BuildSearchExpression (parameter.SearchKey, PagingQueryExtensions.StringContainsMethod, member, others);
            expressions.Add (exp);
            return this;
        }

        public ISearchOptions<TSource> EndsFor (Expression<Func<TSource, string>> member, params Expression<Func<TSource, string>>[] others) {
            if (!parameter.HasSearchKey ()) {
                return this;
            }

            var exp = BuildSearchExpression (parameter.SearchKey, PagingQueryExtensions.StringEndsWithMethod, member, others);
            expressions.Add (exp);
            return this;
        }

        public ISearchOptions<TSource> StartsFor (Expression<Func<TSource, string>> member, params Expression<Func<TSource, string>>[] others) {
            if (!parameter.HasSearchKey ()) {
                return this;
            }

            var exp = BuildSearchExpression (parameter.SearchKey, PagingQueryExtensions.StringStartsWithMethod, member, others);
            expressions.Add (exp);
            return this;
        }

        public static Expression<Func<TSource, bool>> BuildSearchExpression (string searchKey,
            MethodInfo method,
            Expression<Func<TSource, string>> member,
            params Expression<Func<TSource, string>>[] others) {
            var parameter = PagingQueryExtensions.GetParameterExpression (member);
            var logics = member.ToSearchLambda (method, searchKey)
                .Or (others.Select (m => m.ToSearchLambda (method, searchKey)).ToArray ());
            logics = new PagingQueryExtensions.ParameterReplacer (parameter).Visit (logics);
            var expression = Expression.Lambda<Func<TSource, bool>> (logics, false, parameter);
            return expression;
        }
    }

}
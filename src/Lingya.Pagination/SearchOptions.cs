using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lingya.Pagination {
    /// <summary>
    /// 关键字搜索选项
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
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
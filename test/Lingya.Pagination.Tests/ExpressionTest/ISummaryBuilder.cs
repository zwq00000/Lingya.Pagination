using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lingya.Pagination {
    internal interface ISummaryBuilder<TSource> {
        ISummaryBuilder<TSource> Add (string name, int value);

        ISummaryBuilder<TSource> Sum (Expression<Func<TSource, int>> exp);
    }

    internal class SummaryBuilder<TSource> : ISummaryBuilder<TSource> {

        enum AggMethods {
            Count,
            Sum,
            Min,
            Max,
            Average
        }
        enum ValueTypes {
            INT,
            DECIMAL,
            FLOAT,
            DOUBLE
        }
        class SummaryExpression {
            /// <summary>
            /// 名称
            /// </summary>
            /// <value></value>
            public string Name { get; set; }

            /// <summary>
            /// 聚合方法
            /// </summary>
            /// <value></value>
            public AggMethods Method { get; set; }

            public ValueTypes ValueType { get; set; }

            /// <summary>
            /// 聚合表达式
            /// </summary>
            /// <value></value>
            public Expression Expression { get; set; }
        }
        private ISet<SummaryExpression> summaryMap = new HashSet<SummaryExpression> ();
        public ISummaryBuilder<TSource> Add (string name, int value) {
            throw new NotImplementedException ();
        }

        public ISummaryBuilder<TSource> Sum (Expression<Func<TSource, int>> selector) {
            var name = selector.Name;
            IQueryable<TSource> source = null;
            var exp = new SummaryExpression () {
                Name = name,
                ValueType = ValueTypes.INT,
                Method = AggMethods.Sum,
                Expression = Expression.Call (null,
                GetMethodInfo (
                new Func<IQueryable<TSource>, Expression<Func<TSource, int>>, int > (
                Queryable.Sum), source, selector), new Expression[] {
                source.Expression,
                Expression.Quote (selector)
                })
            };
            this.summaryMap.Add (exp);

            return this;
        }

        public IDictionary<string, object> Build (IQueryable<TSource> source, Expression<Func<TSource, long?>> selector) {
            // source.Sum(t=>t.GetHashCode());
            var sum = Expression.Call (null,
                GetMethodInfo (
                    new Func<IQueryable<TSource>, Expression<Func<TSource, long?>>, long? > (
                        Queryable.Sum), source, selector), new Expression[] {
                    source.Expression,
                        Expression.Quote (selector)
                });
            return null;
        }

        private static MethodInfo GetMethodInfo<T1, T2> (Func<T1, T2> f, T1 unused1) {
            return f.Method;
        }
        private static MethodInfo GetMethodInfo<T1, T2, T3> (Func<T1, T2, T3> f, T1 unused1, T2 unused2) {
            return f.Method;
        }
    }

    internal static class SummaryBuilderExtensions {

        /// <summary>
        /// 生成 .where(u=>...) 方法调用表达式
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static MethodCallExpression MakeSumExpression<TSource> (this IQueryable<TSource> source,
            Expression<Func<TSource, int>> selector) {
            //生成 .where(u=>...) 方法调用表达式
            return Expression.Call (
                typeof (Queryable), nameof (Queryable.Where),
                new Type[] { source.ElementType },
                source.Expression, selector);
        }

        private static Expression<Func<TSource, bool>> ToSumLambda<TSource> (this Expression<Func<TSource, int>> expression,
            MethodInfo method,
            string searchKey) {
            var memberAccess = expression.Body;
            var leftParameter = GetParameterExpression (memberAccess);
            var constant = Expression.Constant (searchKey);
            var nullConstant = Expression.Constant (null);
            var notNullExp = Expression.ReferenceNotEqual (memberAccess, nullConstant);
            return Expression.Lambda<Func<TSource, bool>> (
                Expression.AndAlso (notNullExp, Expression.Call (memberAccess, method, constant)), false, leftParameter);
        }

        /// <summary>
        /// 搜索表达式树,查找并返回 参数表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static ParameterExpression GetParameterExpression (Expression expression) {
            switch (expression) {
                case LambdaExpression lambda:
                    return GetParameterExpression (lambda.Body);
                case MemberExpression member:
                    return GetParameterExpression (member.Expression);
                case ParameterExpression parameter:
                    return parameter;
                case MethodCallExpression callExpression:
                    return GetParameterExpression (callExpression.Object);
                default:
                    throw new NotSupportedException ($"不支持的表达式类型:{expression.NodeType}\n表达式:{expression}");
            }
        }
    }

}
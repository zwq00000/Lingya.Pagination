using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lingya.Pagination {

    /// <summary>
    /// 分页查询 扩展方法
    /// </summary>
    internal static class PagingQueryExtensions {

        internal class ParameterReplacer : ExpressionVisitor {
            private readonly ParameterExpression _parameter;

            protected override Expression VisitParameter (ParameterExpression node) {
                return base.VisitParameter (_parameter);
            }

            internal ParameterReplacer (ParameterExpression parameter) {
                _parameter = parameter;
            }
        }
        internal static readonly MethodInfo StringContainsMethod = typeof (string).GetMethod (nameof (string.Contains), new Type[] { typeof (string) });
        internal static readonly MethodInfo StringStartsWithMethod = typeof (string).GetMethod (nameof (string.StartsWith), new Type[] { typeof (string) });
        internal static readonly MethodInfo StringEndsWithMethod = typeof (string).GetMethod (nameof (string.EndsWith), new Type[] { typeof (string) });

        public static IQueryable<TSource> Contains<TSource> (this IQueryable<TSource> query,
            string searchKey,
            Expression<Func<TSource, string>> member,
            params Expression<Func<TSource, string>>[] others) {
            var method = StringContainsMethod;
            return query.BuildSearchQuery (searchKey, method, member, others);
        }

        public static IQueryable<TSource> StartsWith<TSource> (this IQueryable<TSource> query,
            string searchKey,
            Expression<Func<TSource, string>> member,
            params Expression<Func<TSource, string>>[] others) {
            var method = StringStartsWithMethod;
            return query.BuildSearchQuery (searchKey, method, member, others);
        }

        public static IQueryable<TSource> EndsWith<TSource> (this IQueryable<TSource> query,
            string searchKey,
            Expression<Func<TSource, string>> member,
            params Expression<Func<TSource, string>>[] others) {
            return query.BuildSearchQuery (searchKey, StringEndsWithMethod, member, others);
        }

        /// <summary>
        /// 生成 .where(u=>...) 方法调用表达式
        /// </summary>
        /// <param name="source"></param>
        /// <param name="whereExpression"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        private static MethodCallExpression MakeWhereExpression<TSource> (this IQueryable<TSource> source,
            Expression<Func<TSource, bool>> whereExpression) {
            //生成 .where(u=>...) 方法调用表达式
            return Expression.Call (
                typeof (Queryable), nameof (Queryable.Where),
                new Type[] { source.ElementType },
                source.Expression, whereExpression);
        }

        private static IQueryable<TSource> BuildSearchQuery<TSource> (this IQueryable<TSource> query,
            string searchKey,
            MethodInfo method,
            Expression<Func<TSource, string>> member,
            params Expression<Func<TSource, string>>[] others) {
            var parameter = GetParameterExpression (member);
            var logics = member.ToSearchLambda (method, searchKey)
                .Or (others.Select (m => m.ToSearchLambda (method, searchKey)).ToArray ());
            logics = new ParameterReplacer (parameter).Visit (logics);
            var expression = Expression.Lambda<Func<TSource, bool>> (logics, false, parameter);
            Debug.WriteLine (expression);
            return query.Where (expression);
        }

        public static Expression<Func<TSource, bool>> ToSearchLambda<TSource> (this Expression<Func<TSource, string>> expression,
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

        public static Expression Or<TSource> (this Expression<Func<TSource, bool>> expression, params Expression<Func<TSource, bool>>[] others) {
            var exp = expression.Body;
            foreach (var item in others) {
                exp = Expression.OrElse (exp, item.Body);
            }
            return exp;
        }

        /// <summary>
        /// 向下搜索 成员表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static MemberExpression GetMemberExpression (Expression expression) {
            switch (expression) {
                case MemberExpression member:
                    return member;
                case LambdaExpression lambda:
                    return GetMemberExpression (lambda.Body);
                case UnaryExpression unary:
                    return GetMemberExpression (unary.Operand);
                case MethodCallExpression callExpression:
                    return GetMemberExpression (callExpression.Object);
                default:
                    throw new NotSupportedException ($"不支持的表达式类型:{expression.NodeType}\n表达式:{expression}");
            }
        }

        /// <summary>
        /// 搜索表达式树,查找并返回 参数表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static ParameterExpression GetParameterExpression (Expression expression) {
            switch (expression) {
                case LambdaExpression lambda:
                    // return GetParameterExpression (lambda.Body);
                    return lambda.Parameters.First();
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
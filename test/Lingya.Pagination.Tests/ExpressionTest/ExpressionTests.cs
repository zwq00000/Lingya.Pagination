using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Quic;
using System.Reflection;
using Lingya.Pagination.Tests.Mock;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Lingya.Pagination.Tests.ExpressionTest {
    public class ExpressionTests {
        private readonly ITestOutputHelper output;

        private readonly IQueryable<User> UserQuery;

        public ExpressionTests(ITestOutputHelper outputHelper) {
            this.output = outputHelper;
            //InitMockData (100);
            var context = Mock.TestDbContext.UseSqlite();
            UserQuery = context.Users;
        }

        [Fact]
        public void TestContainsExpression() {
            var query = UserQuery;
            var result = query.Contains("1", u => u.UserName, u => u.FullName);
            output.WriteLine(result.ToQueryString());
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TestContains() {
            var key = "1";
            Expression<Func<User, bool>> exp = u => (u.FullName != null && u.FullName.Contains(key) || (u.DepName != null && u.DepName.StartsWith(key)));
            output.WriteLine(exp.ToString());

            output.WriteLine(exp.Body.ToString());
        }

        [Fact]
        public void TestOrExpression() {
            Expression<Func<User, bool>> exp1 = u => u.FullName.Contains("1");
            Expression<Func<User, bool>> exp2 = u => u.FullName.Contains("2");
            Expression<Func<User, bool>> exp3 = u => u.FullName.Contains("3");
            var result = exp1.Or(exp2, exp3);
            output.WriteLine(result.ToString());

            var parameter = Expression.Parameter(typeof(User), "u");
            result = new ParameterReplacer(parameter).Visit(result);

            var exp = Expression.Lambda<Func<User, bool>>(result, false, parameter);

            Assert.NotEmpty(UserQuery.Where(exp));
            Assert.Equal(30, UserQuery.Where(exp).Count());
        }

        [Fact]
        public void TestEndsWith() {
            UserQuery.EndsWith("12", u => u.FullName);
            Xunit.Assert.NotEmpty(UserQuery.ToArray());

            UserQuery.EndsWith("12", u => u.FullName, u => u.UserName);
            Xunit.Assert.NotEmpty(UserQuery.ToArray());
        }

        [Fact]
        public void TestGetWhereExpression() {
            var exp = UserQuery.GetWhereExpression("1", ExpressionExtensions.StringContainsMethod, u => u.FullName, u => u.DepName);
            Assert.NotNull(exp);
            output.WriteLine(exp.ToString());
            var query = UserQuery.Provider.CreateQuery(exp);
            Assert.NotEmpty(query);
        }

        /// <summary>
        /// 测试 OrderByFields 扩展方法
        /// </summary>
        [Fact]
        public void TestOrderByFields() {
            var query = Mock.TestDbContext.UseSqlite().Orders.AsNoTracking();
            var qs = query.OrderByDescending(o=>o.Address.Street).ThenByDescending(o=>o.Id);
            Assert.Contains("ORDER BY", qs.ToQueryString());
            output.WriteLine(qs.ToQueryString());

            var q2 = query.OrderByFields("address.street,id", true);
            var queryString = q2.ToQueryString();
            Assert.Contains("ORDER BY", queryString);
            output.WriteLine(q2.ToQueryString().ToString());
            Assert.Equal(qs.ToQueryString(),q2.ToQueryString());
        }

        [Fact]
        public void TestMakeQuota() {
            Expression<Func<Order, string>> exp1 = o => o.Address.City;
            output.WriteLine(exp1.ToString());

            var param = Expression.Parameter(typeof(Order), "i");
            var exp2 = Expression.Quote(
                Expression.Lambda(
                 Expression.MakeMemberAccess(
                    Expression.MakeMemberAccess(
                        Expression.MakeMemberAccess(param, GetMember<Order>("Address")), GetMember<StreetAddress>("City")
                    ), GetMember<string>("Length"))
                 , param
                ));
            output.WriteLine(exp2.ToString());

            var fieldName = "Address.City.Length";
            var exp3 = Expression.Lambda(BuildMemberAccess(param, typeof(Order), fieldName.Split('.')), param);
            output.WriteLine(exp3.ToString());
        }

        private MemberExpression? BuildMemberAccess(Expression expression, Type ownerType, params string[] propertyNames) {
            if (!propertyNames.Any()) {
                return null;
            }
            var firstMemberName = propertyNames[0];
            var memberInfo = ownerType.GetProperty(firstMemberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            var exp = Expression.MakeMemberAccess(expression, memberInfo);
            if (propertyNames.Length > 1) {
                return BuildMemberAccess(exp, memberInfo.PropertyType, propertyNames[1..]);
            } else {
                return exp;
            }
        }

        private PropertyInfo GetMember(Type ownerType, string propertyName) {
            return ownerType.GetProperty(propertyName,
                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        }
        private PropertyInfo GetMember<T>(string propertyName) {
            return typeof(T).GetProperty(propertyName,
                 BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
        }
    }

    internal class ParameterReplacer : ExpressionVisitor {
        private readonly ParameterExpression _parameter;

        protected override Expression VisitParameter(ParameterExpression node) {
            return base.VisitParameter(_parameter);
        }

        internal ParameterReplacer(ParameterExpression parameter) {
            _parameter = parameter;
        }
    }

    internal static class ExpressionExtensions {

        public static MethodInfo StringContainsMethod = typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) });
        public static MethodInfo StringStartsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string) });
        public static MethodInfo StringEndsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), new Type[] { typeof(string) });

        public static MethodCallExpression GetWhereExpression<TSource>(this IQueryable<TSource> source,
            string searchKey,
            MethodInfo method,
            Expression<Func<TSource, string>> member,
            params Expression<Func<TSource, string>>[] others) {
            var parameter = GetParameterExpression(member);
            var logics = member.ToSearchLambda(method, searchKey)
                .Or(others.Select(m => m.ToSearchLambda(method, searchKey)).ToArray());
            logics = new ParameterReplacer(parameter).Visit(logics);
            var whereExpression = Expression.Lambda<Func<TSource, bool>>(logics, false, parameter);
            Debug.WriteLine(whereExpression);

            //生成 .where(u=>...) 方法调用表达式
            return Expression.Call(
                typeof(Queryable), nameof(Queryable.Where),
                new Type[] { source.ElementType },
                source.Expression, whereExpression);
        }

        public static IQueryable<TSource> Contains<TSource>(this IQueryable<TSource> query,
            string searchKey,
            Expression<Func<TSource, string>> containsMember,
            params Expression<Func<TSource, string>>[] containsMembers) {
            var method = StringContainsMethod;
            var parameter = GetParameterExpression(containsMember);
            var logics = containsMember.ToSearchLambda(method, searchKey).
            Or(containsMembers.Select(e => e.ToSearchLambda(method, searchKey)).ToArray());

            logics = (BinaryExpression)new ParameterReplacer(parameter).Visit(logics);
            var expression = Expression.Lambda<Func<TSource, bool>>(logics, false, parameter);
            Debug.WriteLine(expression);
            return query.Where(expression);
        }

        public static IQueryable<TSource> StartsWith<TSource>(this IQueryable<TSource> query,
            string searchKey,
            Expression<Func<TSource, string>> containsMember,
            params Expression<Func<TSource, string>>[] containsMembers) {
            var method = StringStartsWithMethod;
            var parameter = GetParameterExpression(containsMember);
            containsMember.ToSearchLambda(method, searchKey);
            var logics = containsMember.ToSearchLambda(method, searchKey)
                .Or(containsMembers.Select(e => e.ToSearchLambda(method, searchKey)).ToArray());
            logics = (BinaryExpression)new ParameterReplacer(parameter).Visit(logics);
            var expression = Expression.Lambda<Func<TSource, bool>>(logics, false, parameter);
            Debug.WriteLine(expression);
            return query.Where(expression);
        }

        public static IQueryable<TSource> EndsWith<TSource>(this IQueryable<TSource> query,
            string searchKey,
            Expression<Func<TSource, string>> containsMember,
            params Expression<Func<TSource, string>>[] containsMembers) {
            var logics = containsMember.ToEndsWithExpression(searchKey).Or(containsMembers.Select(e => ToEndsWithExpression(e, searchKey)).ToArray());
            var expression = Expression.Lambda<Func<TSource, bool>>(logics, false, GetParameterExpression(containsMember));
            Debug.WriteLine(expression);
            return query.Where(expression);
        }

        public static Expression<Func<TSource, bool>> ToSearchLambda<TSource>(this Expression<Func<TSource, string>> containsMember,
            MethodInfo method,
            string searchKey) {
            var memberAccess = GetMemberExpression(containsMember);
            var leftParameter = GetParameterExpression(memberAccess);
            var constant = Expression.Constant(searchKey);
            var nullConstant = Expression.Constant(null);
            var notNullExp = Expression.ReferenceNotEqual(memberAccess, nullConstant);
            return Expression.Lambda<Func<TSource, bool>>(
                Expression.AndAlso(notNullExp, Expression.Call(memberAccess, method, constant)), false, leftParameter);
        }

        private static Expression<Func<TSource, bool>> ToContainsExpression<TSource>(this Expression<Func<TSource, string>> containsMember, string searchKey) {
            var memberAccess = GetMemberExpression(containsMember);
            var parameter = GetParameterExpression(memberAccess);
            var constant = Expression.Constant(searchKey);
            return Expression.Lambda<Func<TSource, bool>>(Expression.Call(memberAccess, StringContainsMethod, constant), false, parameter);
        }

        public static Expression<Func<TSource, bool>> ToStartsWithExpression<TSource>(this Expression<Func<TSource, string>> containsMember, string searchKey) {
            var memberAccess = GetMemberExpression(containsMember);
            var leftParameter = GetParameterExpression(memberAccess);
            var method = typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string) });
            var constant = Expression.Constant(searchKey);
            return Expression.Lambda<Func<TSource, bool>>(Expression.Call(memberAccess, method, constant), false, leftParameter);
        }

        public static Expression<Func<TSource, bool>> ToEndsWithExpression<TSource>(this Expression<Func<TSource, string>> containsMember, string searchKey) {
            var memberAccess = GetMemberExpression(containsMember);
            var leftParameter = GetParameterExpression(memberAccess);
            var method = typeof(string).GetMethod(nameof(string.EndsWith), new Type[] { typeof(string) });
            var constant = Expression.Constant(searchKey);
            return Expression.Lambda<Func<TSource, bool>>(Expression.Call(memberAccess, method, constant), false, leftParameter);
        }

        public static Expression<Func<TSource, bool>> ToContainsExpression<TSource>(this Expression<Func<TSource, string>> containsMember, ParameterExpression rightParameter) {
            var memberAccess = GetMemberExpression(containsMember);
            var leftParameter = GetParameterExpression(memberAccess);
            var method = typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) });
            //var constant = Expression.Constant (searchKey);
            var call = Expression.Call(memberAccess, method, rightParameter);
            return Expression.Lambda<Func<TSource, bool>>(call, false, leftParameter);
        }

        public static Expression Or<TSource>(this Expression<Func<TSource, bool>> expression, params Expression<Func<TSource, bool>>[] others) {
            var exp = (Expression)expression.Body;

            foreach (var item in others) {
                exp = Expression.OrElse(exp, item.Body);
                //((MemberExpression)((MethodCallExpression) item.Body).Object).Expression = expression.Parameters[0];
            }
            return exp;
        }

        /// <summary>
        /// 向下搜索 成员表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static MemberExpression GetMemberExpression(Expression expression) {
            switch (expression) {
                case MemberExpression member:
                    return member;
                case LambdaExpression lambda:
                    return GetMemberExpression(lambda.Body);
                case UnaryExpression unary:
                    return GetMemberExpression(unary.Operand);
                case MethodCallExpression callExpression:
                    return GetMemberExpression(callExpression.Object);
                default:
                    throw new NotSupportedException($"不支持的表达式类型:{expression.NodeType}\n表达式:{expression}");
            }
        }

        /// <summary>
        /// 搜索表达式树,查找并返回 参数表达式
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static ParameterExpression GetParameterExpression(Expression expression) {
            switch (expression) {
                case LambdaExpression lambda:
                    return GetParameterExpression(lambda.Body);
                case MemberExpression member:
                    return GetParameterExpression(member.Expression);
                case ParameterExpression parameter:
                    return parameter;
                case MethodCallExpression callExpression:
                    return GetParameterExpression(callExpression.Object);
                default:
                    throw new NotSupportedException($"不支持的表达式类型:{expression.NodeType}\n表达式:{expression}");
            }
        }

    }
}
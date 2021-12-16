using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Lingya.Pagination.Tests.Mock;
using Xunit;
using Xunit.Abstractions;

namespace Lingya.Pagination.Tests.ExpressionTest {
    public class ExpressionTests {
        private ITestOutputHelper _output;

        private readonly IQueryable<User> UserQuery;
        private IList<User> users = new List<User> {
            new User ("user1", "User 1")
        };

        private void InitMockData (int count) {
            for (int i = 0; i < count; i++) {
                users.Add (new User ($"user_{i}", $"UserName {i}"));
            }
        }

        public ExpressionTests (Xunit.Abstractions.ITestOutputHelper outputHelper) {
            this._output = outputHelper;
            //InitMockData (100);
            var context = Mock.TestDbContext.UseInMemory ();
            UserQuery = context.Users;
        }

        [Fact]
        public void TestContainsExpression () {
            //var query = users.AsQueryable ();
            var query = UserQuery;
            // var exp = ExpressionExtensions.ToContainsExpression<User> (u => u.FullName, Expression.MakeMemberAccess(typeof(User),"u"), "1");
            // Assert.NotNull (exp);
            // _output.WriteLine (exp.ToString ());
            // query = query.Where(exp);
            // Assert.NotEmpty (query);
            // Assert.Equal (10,query.Count());

            // var searchKey = "1";
            // //Expression.Bind()
            // Expression.Variable (typeof (string));
            // exp = ExpressionExtensions.ToContainsExpression<User> (u => u.FullName, Expression.Variable (typeof (string), nameof (searchKey)));
            // Assert.NotNull (exp);
            // _output.WriteLine (exp.ToString ());
            // Xunit.Assert.NotEmpty (query.Where (exp));
        }

        [Fact]
        public void TestContains () {
            var key = "1";
            Expression<Func<User, bool>> exp = u => (u.FullName!=null && u.FullName.Contains (key) || (u.DepName!=null && u.DepName.StartsWith(key)));
            _output.WriteLine (exp.ToString ());

            _output.WriteLine (exp.Body.ToString ());
        }

        [Fact]
        public void TestOrExpression () {
            Expression<Func<User, bool>> exp1 = u => u.FullName.Contains ("1");
            Expression<Func<User, bool>> exp2 = u => u.FullName.Contains ("2");
            Expression<Func<User, bool>> exp3 = u => u.FullName.Contains ("3");
            var result = exp1.Or (exp2, exp3);
            _output.WriteLine (result.ToString ());

            var parameter = Expression.Parameter (typeof (User), "u");
            result = new ParameterReplacer (parameter).Visit (result);

            var exp = Expression.Lambda<Func<User, bool>> (result, false, parameter);

            Assert.NotEmpty (UserQuery.Where (exp));
            Assert.Equal (30, UserQuery.Where (exp).Count ());
        }

        [Fact]
        public void TestEndsWith () {
            UserQuery.EndsWith ("12", u => u.FullName);
            Xunit.Assert.NotEmpty (UserQuery.ToArray ());

            UserQuery.EndsWith ("12", u => u.FullName, u => u.UserName);
            Xunit.Assert.NotEmpty (UserQuery.ToArray ());
        }

        [Fact]
        public void TestGetWhereExpression () {
            var exp = UserQuery.GetWhereExpression ("1", ExpressionExtensions.StringContainsMethod, u => u.FullName, u => u.DepName);
            Assert.NotNull (exp);
            _output.WriteLine (exp.ToString ());
            var query = UserQuery.Provider.CreateQuery (exp);
            Assert.NotEmpty (query);
        }
    }

    internal class ParameterReplacer : ExpressionVisitor {
        private readonly ParameterExpression _parameter;

        protected override Expression VisitParameter (ParameterExpression node) {
            return base.VisitParameter (_parameter);
        }

        internal ParameterReplacer (ParameterExpression parameter) {
            _parameter = parameter;
        }
    }

    internal static class ExpressionExtensions {

        public static MethodInfo StringContainsMethod = typeof (string).GetMethod (nameof (string.Contains), new Type[] { typeof (string) });
        public static MethodInfo StringStartsWithMethod = typeof (string).GetMethod (nameof (string.StartsWith), new Type[] { typeof (string) });
        public static MethodInfo StringEndsWithMethod = typeof (string).GetMethod (nameof (string.EndsWith), new Type[] { typeof (string) });

        public static MethodCallExpression GetWhereExpression<TSource> (this IQueryable<TSource> source,
            string searchKey,
            MethodInfo method,
            Expression<Func<TSource, string>> member,
            params Expression<Func<TSource, string>>[] others) {
            var parameter = GetParameterExpression (member);
            var logics = member.ToSearchLambda (method, searchKey)
                .Or (others.Select (m => m.ToSearchLambda (method, searchKey)).ToArray ());
            logics = new ParameterReplacer (parameter).Visit (logics);
            var whereExpression = Expression.Lambda<Func<TSource, bool>> (logics, false, parameter);
            Debug.WriteLine (whereExpression);

            //生成 .where(u=>...) 方法调用表达式
            return Expression.Call (
                typeof (Queryable), nameof (Queryable.Where),
                new Type[] { source.ElementType},
                source.Expression,whereExpression);
        }

        public static IQueryable<TSource> Contains<TSource> (this IQueryable<TSource> query,
            string searchKey,
            Expression<Func<TSource, string>> containsMember,
            params Expression<Func<TSource, string>>[] containsMembers) {
            var method = StringStartsWithMethod;
            var parameter = GetParameterExpression (containsMember);
            var logics = containsMember.ToSearchLambda (method, searchKey).
            Or (containsMembers.Select (e => e.ToSearchLambda (method, searchKey)).ToArray ());

            logics = (BinaryExpression) new ParameterReplacer (parameter).Visit (logics);
            var expression = Expression.Lambda<Func<TSource, bool>> (logics, false, parameter);
            Debug.WriteLine (expression);
            return query.Where (expression);
        }

        public static IQueryable<TSource> StartsWith<TSource> (this IQueryable<TSource> query,
            string searchKey,
            Expression<Func<TSource, string>> containsMember,
            params Expression<Func<TSource, string>>[] containsMembers) {
            var method = StringStartsWithMethod;
            var parameter = GetParameterExpression (containsMember);
            containsMember.ToSearchLambda (method, searchKey);
            var logics = containsMember.ToSearchLambda (method, searchKey)
                .Or (containsMembers.Select (e => e.ToSearchLambda (method, searchKey)).ToArray ());
            logics = (BinaryExpression) new ParameterReplacer (parameter).Visit (logics);
            var expression = Expression.Lambda<Func<TSource, bool>> (logics, false, parameter);
            Debug.WriteLine (expression);
            return query.Where (expression);
        }

        public static IQueryable<TSource> EndsWith<TSource> (this IQueryable<TSource> query,
            string searchKey,
            Expression<Func<TSource, string>> containsMember,
            params Expression<Func<TSource, string>>[] containsMembers) {
            var logics = containsMember.ToEndsWithExpression (searchKey).Or (containsMembers.Select (e => ToEndsWithExpression (e, searchKey)).ToArray ());
            var expression = Expression.Lambda<Func<TSource, bool>> (logics, false, GetParameterExpression (containsMember));
            Debug.WriteLine (expression);
            return query.Where (expression);
        }

        public static Expression<Func<TSource, bool>> ToSearchLambda<TSource> (this Expression<Func<TSource, string>> containsMember,
            MethodInfo method,
            string searchKey) {
            var memberAccess = GetMemberExpression (containsMember);
            var leftParameter = GetParameterExpression (memberAccess);
            var constant = Expression.Constant (searchKey);
            var nullConstant = Expression.Constant(null);
            var notNullExp = Expression.ReferenceNotEqual(memberAccess,nullConstant);
            return Expression.Lambda<Func<TSource, bool>> (
                Expression.AndAlso(notNullExp,Expression.Call (memberAccess, method, constant)), false, leftParameter);
        }

        private static Expression<Func<TSource, bool>> ToContainsExpression<TSource> (this Expression<Func<TSource, string>> containsMember, string searchKey) {
            var memberAccess = GetMemberExpression (containsMember);
            var parameter = GetParameterExpression (memberAccess);
            var constant = Expression.Constant (searchKey);
            return Expression.Lambda<Func<TSource, bool>> (Expression.Call (memberAccess, StringContainsMethod, constant), false, parameter);
        }

        public static Expression<Func<TSource, bool>> ToStartsWithExpression<TSource> (this Expression<Func<TSource, string>> containsMember, string searchKey) {
            var memberAccess = GetMemberExpression (containsMember);
            var leftParameter = GetParameterExpression (memberAccess);
            var method = typeof (string).GetMethod (nameof (string.StartsWith), new Type[] { typeof (string) });
            var constant = Expression.Constant (searchKey);
            return Expression.Lambda<Func<TSource, bool>> (Expression.Call (memberAccess, method, constant), false, leftParameter);
        }

        public static Expression<Func<TSource, bool>> ToEndsWithExpression<TSource> (this Expression<Func<TSource, string>> containsMember, string searchKey) {
            var memberAccess = GetMemberExpression (containsMember);
            var leftParameter = GetParameterExpression (memberAccess);
            var method = typeof (string).GetMethod (nameof (string.EndsWith), new Type[] { typeof (string) });
            var constant = Expression.Constant (searchKey);
            return Expression.Lambda<Func<TSource, bool>> (Expression.Call (memberAccess, method, constant), false, leftParameter);
        }

        public static Expression<Func<TSource, bool>> ToContainsExpression<TSource> (this Expression<Func<TSource, string>> containsMember, ParameterExpression rightParameter) {
            var memberAccess = GetMemberExpression (containsMember);
            var leftParameter = GetParameterExpression (memberAccess);
            var method = typeof (string).GetMethod (nameof (string.Contains), new Type[] { typeof (string) });
            //var constant = Expression.Constant (searchKey);
            var call = Expression.Call (memberAccess, method, rightParameter);
            return Expression.Lambda<Func<TSource, bool>> (call, false, leftParameter);
        }

        public static Expression Or<TSource> (this Expression<Func<TSource, bool>> expression, params Expression<Func<TSource, bool>>[] others) {
            var exp = (Expression) expression.Body;

            foreach (var item in others) {
                exp = Expression.OrElse (exp,item.Body);
                //((MemberExpression)((MethodCallExpression) item.Body).Object).Expression = expression.Parameters[0];
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
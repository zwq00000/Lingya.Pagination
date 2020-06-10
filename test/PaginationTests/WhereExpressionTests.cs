using System;
using System.Linq;
using System.Linq.Expressions;
using Lingya.Pagination;
using PaginationTests.Mock;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace PaginationTests {
    public class WhereExpressionTests {

        [Fact]
        public void TestExperssion() {
            var users = new User[0];

            var query = users.AsQueryable(); //CreateTestQuery(users.AsQueryable());
            Assert.NotNull(query.Expression);
            var ordering = new Ordering(query.Expression,OrderingDirection.Asc);
            var queryModel = CreateQueryModel<User>(query);
            //var orderByClause = ExpressionHelper.CreateOrderByClause();
            
            //ordering.Accept( queryModel, ordering, 1);

            //Assert.Equal("", ordering.ToString());
        }


        public static QueryModel CreateQueryModel<T>(IQueryable<T> queryable) {
            return CreateQueryModel(CreateMainFromClause_Int("s", typeof(T), queryable));
        }
        public static QueryModel CreateQueryModel(MainFromClause mainFromClause) {
            var selectClause = new SelectClause(new QuerySourceReferenceExpression(mainFromClause));
            return new QueryModel(mainFromClause, selectClause);
        }

        public static MainFromClause CreateMainFromClause_Int(string itemName, Type itemType, IQueryable querySource) {
                return new MainFromClause(itemName, itemType, Expression.Constant(querySource));
        }

        public IQueryable<User> CreateTestQuery(IQueryable<User> users) {
            return users.Where(u => u.FullName.Contains("a"));
        }
    }

    public class FilterExpressionTests
    {
        private ITestOutputHelper _output;

        public FilterExpressionTests(Xunit.Abstractions.ITestOutputHelper output) {
            this._output = output;
        }

        // [Fact]
        // public void TestFilterExpressionBuilder() {
        //     var users = new User[0];
        //     var query = users.AsQueryable();
        //     var expression = FilterExtensions.GenFilterExpression(query,"a",nameof(User.FullName),nameof(User.UserName));
        //     _output.WriteLine(expression.ToString());
        // }

        [Fact]
        public void TestToContains() {
            var para =  Expression.Parameter(typeof(User), "u");
            var member = Expression.MakeMemberAccess(para, typeof(User).GetProperty(nameof(User.FullName)));
            //var exp = FilterExtensions.ToContains<User>(member, "test");
            //_output.WriteLine(exp.ToString());
        }

        // [Fact]
        // public void TestFilter() {
        //     var users = new User[]{
        //         new User(){FullName = "abc",UserName="Abc"},
        //         new User(){FullName = "abc",UserName="123Abc"}
        //     };
        //     var query = users.AsQueryable();
        //     Assert.NotNull(query.Expression);
        //     var filted = query.Filter(new PageParameter() { SearchKey = "A" });
        //     Assert.NotNull(filted);
        //     Assert.Equal(2, filted.Count());
        // }

        [Fact]
        public void TestFilterExpression() {
            var users = new User[0];

            var query = users.AsQueryable(); //CreateTestQuery(users.AsQueryable());
            Assert.NotNull(query.Expression);
            query.Where(u => u.FullName.Contains("a"));
            var expression = FilterExpression();
            _output.WriteLine(expression.ToString());
            Assert.Equal("", expression.ToString());
        }


        private Expression<Func<User, bool>> FilterExpression() {
            return u => u.FullName.Contains("a");
        }
    }
}
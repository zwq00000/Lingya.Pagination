using System;
using System.Linq;
using System.Linq.Expressions;
using Lingya.Pagination.Tests.Mock;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Xunit;

namespace Lingya.Pagination.Tests {
    public class WhereExpressionTests {

        [Fact]
        public void TestExperssion () {
            var users = Array.Empty<User> ();

            var query = users.AsQueryable (); //CreateTestQuery(users.AsQueryable());
            Assert.NotNull (query.Expression);
            var ordering = new Ordering (query.Expression, OrderingDirection.Asc);
            var queryModel = CreateQueryModel<User> (query);
            //var orderByClause = ExpressionHelper.CreateOrderByClause();

            //ordering.Accept( queryModel, ordering, 1);

            //Assert.Equal("", ordering.ToString());
        }

        public static QueryModel CreateQueryModel<T> (IQueryable<T> queryable) {
            return CreateQueryModel (CreateMainFromClause_Int ("s", typeof (T), queryable));
        }
        public static QueryModel CreateQueryModel (MainFromClause mainFromClause) {
            var selectClause = new SelectClause (new QuerySourceReferenceExpression (mainFromClause));
            return new QueryModel (mainFromClause, selectClause);
        }

        public static MainFromClause CreateMainFromClause_Int (string itemName, Type itemType, IQueryable querySource) {
            return new MainFromClause (itemName, itemType, Expression.Constant (querySource));
        }

        public static IQueryable<User> CreateTestQuery (IQueryable<User> users) {
            return users.Where (u => u.FullName.Contains ("a"));
        }
    }
}
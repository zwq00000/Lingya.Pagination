using System;
using System.Linq.Expressions;
using Lingya.Pagination.Tests.Mock;
using Xunit;
using Xunit.Abstractions;

namespace Lingya.Pagination.Tests {
    public class FilterExpressionTests {
        private readonly ITestOutputHelper output;

        public FilterExpressionTests (ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void TestPredicateExpression () {
            Expression<Func<User, bool>> expression = u => u.FullName.Contains ("a");
            output.WriteLine (expression.ToString ());
            Assert.Equal ("u => u.FullName.Contains(\"a\")", expression.ToString ());
        }
    }
}
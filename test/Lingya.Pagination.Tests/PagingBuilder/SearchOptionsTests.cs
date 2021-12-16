using System.Linq;
using Lingya.Pagination;
using Lingya.Pagination.Tests.Mock;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Lingya.Pagination.Tests {
    public class SearchOptionsTests {
        private readonly ITestOutputHelper output;

        public SearchOptionsTests (ITestOutputHelper outputHelper) {
            this.output = outputHelper;
        }

        [Fact]
        public void TesBuildExpression () {
            var options = new SearchOptions<User> (new PageParameter { SearchKey = "2" });
            options.ContainsFor (u => u.UserName)
                .StartsFor (u => u.DepName)
                .EndsFor (u => u.DepName);
            var exp = options.BuildExpression ();
            output.WriteLine (exp.ToString ());
            Assert.NotNull (exp);

            var query = TestDbContext.UseSqlite ().Users.Where (exp);
            Assert.NotEmpty (query);
            output.WriteLine (query.ToQueryString ());
        }

        [Fact]
        public void TestSum () {
            var context = TestDbContext.UseSqlite ();
            var accounts = context.Accounts;
            Assert.NotEmpty (accounts);
            var query = accounts.GroupBy (g => true).Select (g =>
                new {
                    quantity = g.Sum (a => a.Quantity),
                        price = g.Sum (a => a.Price)
                });
            output.WriteLine (query.ToQueryString ());
            output.WriteJson (query.First ());
        }
    }

    public interface ISummaryOptions<TSource> {

    }
}
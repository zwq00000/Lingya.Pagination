using System.Linq;
using Lingya.Pagination;
using Lingya.Pagination.Tests.Mock;
using Xunit;
using Xunit.Abstractions;

namespace Lingya.Pagination.Tests {
    public class PagingBuilderTests {
        private readonly ITestOutputHelper output;
        private readonly TestDbContext context;

        public PagingBuilderTests (ITestOutputHelper outputHelper) {
            this.output = outputHelper;
            this.context = TestDbContext.UseInMemory ();
        }

        [Fact]
        public void TestBuild () {
            var parameter = new PageParameter ();
            var result = context.Users.AsQueryable ().PagingBuilder (parameter)
                .Search (opt => opt.ContainsFor (u => u.FullName, u => u.UserName))
                .ToPaging ();
            Assert.NotNull (result);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (100, result.Page.Total);
        }

        [Fact]
        public void TestSearchKey () {
            var parameter = new PageParameter () { SearchKey = "2" };
            var result = context.Users.AsQueryable ().PagingBuilder (parameter)
                .Search (opt => opt.ContainsFor (u => u.FullName, u => u.UserName))
                .ToPaging ();
            Assert.NotNull (result);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (19, result.Page.Total);
            Assert.Equal (19, result.Values.Count ());
        }

        [Fact]
        public void TestSearchDateTime () {
            var parameter = new PageParameter () { SearchKey = "20" };
            var result = context.Users.AsQueryable ().PagingBuilder (parameter)
                .Search (opt => opt.ContainsFor (u => u.CreatedDate.ToShortDateString ()))
                .ToPaging ();
            Assert.NotNull (result);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (1, result.Page.Page);
            Assert.NotEmpty (result.Values);
        }

        [Fact]
        public void TestSearchOptions () {
            var parameter = new PageParameter () { SearchKey = "2" };
            var query = context.Users.AsQueryable ();
            var result = query.PagingBuilder (parameter)
                .Search (b => {
                    b.ContainsFor (u => u.FullName, u => u.UserName);
                })
                .ToPaging ();
            Assert.NotNull (result);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (19, result.Page.Total);
            Assert.Equal (19, result.Values.Count ());
        }

        [Fact]
        public void TestWithAggregate () {
            var parameter = new PageParameter () { SearchKey = "2", PageSize = 10 };
            var query = context.Accounts;
            var result = query.PagingBuilder (parameter).WithAggregate (g => new {
                count = g.Count (),
                    quantity = g.Sum (a => a.Quantity),
                    price = g.Sum (a => a.Price)
            });
            Assert.NotNull (result);
            output.WriteJson (result);

        }
    }
}
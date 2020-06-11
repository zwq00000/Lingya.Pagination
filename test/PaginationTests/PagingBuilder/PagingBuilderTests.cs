using System.Linq;
using Lingya.Pagination;
using PaginationTests.Mock;
using Xunit;

public class PagingBuilderTests {
    private readonly TestDbContext context;

    public PagingBuilderTests () {
        this.context = TestDbContext.GetMock ();
    }

    [Fact]
    public void TestBuild () {
        var parameter = new PageParameter ();
        var result = context.Users.AsQueryable ().PagingBuilder (parameter)
            .ContainsFor (u => u.FullName, u => u.UserName).ToPaging ();
        Assert.NotNull (result);
        Assert.Equal (20, result.Page.PageSize);
        Assert.Equal (1, result.Page.Page);
        Assert.Equal (100, result.Page.Total);
    }

    [Fact]
    public void TestSearchKey () {
        var parameter = new PageParameter () { SearchKey = "2" };
        var result = context.Users.AsQueryable ().PagingBuilder (parameter)
            .ContainsFor (u => u.FullName, u => u.UserName).ToPaging ();
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
            .ContainsFor (u => u.CreatedDate.ToShortDateString()).ToPaging ();
        Assert.NotNull (result);
        Assert.Equal (20, result.Page.PageSize);
        Assert.Equal (1, result.Page.Page);
        Assert.NotEmpty(result.Values);
    }
}
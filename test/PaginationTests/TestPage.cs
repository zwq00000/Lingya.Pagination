using System;
using System.Collections.Generic;
using System.Linq;
using Lingya.Pagination;
using Microsoft.EntityFrameworkCore;
using PaginationTests.Mock;
using Xunit;

namespace PaginationTests {
    public class TestPage {
        private readonly TestDbContext _context;

        public TestPage()
        {
            var builder = new DbContextOptionsBuilder<TestDbContext>();
            builder.UseInMemoryDatabase("testDb");
            this._context = new TestDbContext(builder.Options);
            InitDatabase();
        }

        private void InitDatabase()
        {
            _context.Users.AddRange(CreateMockData().ToArray());
            _context.SaveChanges();

        }

        private IEnumerable<User> CreateMockData(int count = 100)
        {
            for (int i = 0; i < count; i++)
            {
                yield return  new User(){UserName = $"name_{i}",FullName = $"Full Name {i}",CreatedDate = DateTime.Now.AddDays(-i)};
            }
        }

        [Fact]
        public async void TestNullParameter()
        {
            var result = await this._context.Users.PagingAsync(null);
            Assert.NotNull(result);
            Assert.Equal(1,result.Page.Page);
            Assert.Equal(20, result.Page.PageSize);
            Assert.Equal(20, result.Values.Count());
        }


        [Fact]
        public async void TestSortDesc() {
            var parameter = new PageParamete(){SortBy = "username",Descending = true};
            var result = await this._context.Users.PagingAsync(parameter);
            Assert.NotNull(result);
            Assert.Equal(1, result.Page.Page);
            Assert.Equal(20, result.Page.PageSize);
            Assert.Equal(20, result.Values.Count());
            Assert.Equal("name_99",result.Values.First().UserName);
        }

        [Fact]
        public async void TestSort() {
            var parameter = new PageParamete() { SortBy = "createdDate" };
            var result = await this._context.Users.PagingAsync(parameter);
            Assert.NotNull(result);
            Assert.Equal(1, result.Page.Page);
            Assert.Equal(20, result.Page.PageSize);
            Assert.Equal(20, result.Values.Count());
            Assert.Equal("name_99", result.Values.First().UserName);
        }


        [Fact]
        public async void TestSearchKey()
        {
            const string searchKey = "name_12";
            var parameter = new PageParamete() {SearchKey = searchKey};

            var result = await this._context.Users.Where(u=>u.UserName.StartsWith(searchKey)||u.FullName.StartsWith(searchKey)).PagingAsync(parameter);
            Assert.NotNull(result);
            Assert.Equal(1, result.Page.Page);
            Assert.Equal(20, result.Page.PageSize);
            Assert.Equal(1,result.Page.Total);
            Assert.Equal(1, result.Values.Count());
            Assert.Equal("name_12", result.Values.First().UserName);
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
<<<<<<< HEAD
using System.Linq.Expressions;
using Lingya.Pagination;
using Lingya.Pagination.Tests.Mock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
=======
using Lingya.Pagination.Tests.Mock;
>>>>>>> af9c08603256dd2d65573c09bca64f6b666b9013
using Xunit;

namespace Lingya.Pagination.Tests {
    public class TestPaginationAsync {

<<<<<<< HEAD
        public TestPaginationAsync () {
            _context = TestDbContext.UseInMemory ();
        }
=======
        public TestPaginationAsync () { }
>>>>>>> af9c08603256dd2d65573c09bca64f6b666b9013

        [Fact]
        public async void TestNullParameter () {
            var result = await TestDbContext.UseInMemory ().Users.ToPagingAsync (null);
            Assert.NotNull (result);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (20, result.Values.Count ());
        }

        [Fact]
        public async void TestSortDesc () {
            var parameter = new PageParameter () { SortBy = "username", Descending = true };
            var result = await TestDbContext.UseInMemory ().Users.ToPagingAsync (parameter);
            Assert.NotNull (result);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (20, result.Values.Count ());
            Assert.Equal ("name_99", result.Values.First ().UserName);
        }

        [Fact]
        public async void TestSort () {
            var parameter = new PageParameter () { SortBy = "createdDate" };
            var result = await TestDbContext.UseInMemory ().Users.ToPagingAsync (parameter);
            Assert.NotNull (result);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (20, result.Values.Count ());
            Assert.Equal ("name_99", result.Values.First ().UserName);
        }

        [Fact]
        public async void TestMultiFieldSort () {
            var parameter = new PageParameter () { SortBy = "createdDate,fullName" };
            var result = await TestDbContext.UseInMemory ().Users.ToPagingAsync (parameter);
            Assert.NotNull (result);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (20, result.Values.Count ());
            Assert.Equal ("Full Name 9", result.Values.First ().FullName);

            var query = TestDbContext.UseInMemory ().Users.OrderBy (u => u.CreatedDate).ThenBy (u => u.UserName);
            Console.Write (query.Expression);
        }

        [Fact]
        public async void TestSearchKey () {
            const string searchKey = "name_12";
            var parameter = new PageParameter () { SearchKey = searchKey };

            var result = await TestDbContext.UseInMemory ().Users.Where (u => u.UserName.StartsWith (searchKey) || u.FullName.StartsWith (searchKey)).ToPagingAsync (parameter);
            Assert.NotNull (result);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (1, result.Page.Total);
            Assert.NotEmpty (result.Values);
            Assert.Equal ("name_12", result.Values.First ().UserName);
        }

    }
}
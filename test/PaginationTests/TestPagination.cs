using System;
using System.Collections.Generic;
using System.Linq;
using Lingya.Pagination;
using PaginationTests.Mock;
using Xunit;

namespace PaginationTests
{
    public class TestPagination {

        private IEnumerable<User> _users;

        public TestPagination () {
            _users = CreateMockData ();
        }

        private IEnumerable<User> CreateMockData (int count = 100) {
            for (int i = 0; i < count; i++) {
                yield return new User () { UserName = $"name_{i}", FullName = $"Full Name {i % 10}", CreatedDate = DateTime.Now.AddDays (-i) };
            }
        }

        [Fact]
        public void TestNullParameter () {
            var result = _users.ToPaging (null);
            Assert.NotNull (result);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (20, result.Values.Count ());
        }

        [Fact]
        public void TestSortDesc () {
            var parameter = new PageParameter () { SortBy = "username", Descending = true };
            var result = _users.ToPaging (parameter);
            Assert.NotNull (result);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (20, result.Values.Count ());
            Assert.Equal ("name_99", result.Values.First ().UserName);
        }

        [Fact]
        public void TestSort () {
            var parameter = new PageParameter () { SortBy = "createdDate" };
            var result = _users.ToPaging (parameter);
            Assert.NotNull (result);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (20, result.Values.Count ());
            Assert.Equal ("name_99", result.Values.First ().UserName);
        }

        [Fact]
        public void TestMultiFieldSort () {
            var parameter = new PageParameter () { SortBy = "createdDate,fullName" };
            var result = _users.ToPaging (parameter);
            Assert.NotNull (result);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (20, result.Values.Count ());
            Assert.Equal ("Full Name 9", result.Values.First ().FullName);
        }

        [Fact]
        public void TestSearchKey () {
            const string searchKey = "name_12";
            var parameter = new PageParameter () { SearchKey = searchKey };

            var result = _users
                .Where (u => u.UserName.StartsWith (searchKey) || u.FullName.StartsWith (searchKey))
                .ToPaging (parameter);

            Assert.NotNull (result);
            Assert.Equal (1, result.Page.Page);
            Assert.Equal (20, result.Page.PageSize);
            Assert.Equal (1, result.Page.Total);
            Assert.NotEmpty (result.Values);
            Assert.Equal ("name_12", result.Values.First ().UserName);
        }

    }
}
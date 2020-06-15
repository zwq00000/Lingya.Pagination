using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PaginationTests.Mock {
    public class TestDbContext : DbContext {

        public TestDbContext (DbContextOptions<TestDbContext> options) : base (options) {

        }

        public DbSet<User> Users { get; set; }

        public static TestDbContext GetMock () {
            var context = CreateDbContext();
            InitDatabase(context);
            return context;
        }

        public static TestDbContext CreateDbContext () {
            var builder = new DbContextOptionsBuilder<TestDbContext> ();
            builder.UseLoggerFactory (new LoggerFactory ().AddDebug (LogLevel.Trace));
            builder.UseInMemoryDatabase ("testDb");
            return new TestDbContext (builder.Options);
        }

        private static void InitDatabase (TestDbContext context) {
            if (!context.Users.Any ()) {
                context.Users.AddRange (CreateMockData ().ToArray ());
                context.SaveChanges ();
            }
        }

        private static IEnumerable<User> CreateMockData (int count = 100) {
            for (int i = 0; i < count; i++) {
                yield return new User () { UserName = $"name_{i}", FullName = $"Full Name {i % 10}", CreatedDate = DateTime.Now.AddDays (-i) };
            }
        }
    }
}
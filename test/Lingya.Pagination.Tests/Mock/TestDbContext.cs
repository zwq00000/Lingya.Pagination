using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Lingya.Pagination.Tests.Mock {
    public class TestDbContext : DbContext {

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) {

        }

        public DbSet<User> Users { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Order> Orders { get; set; }

        public static TestDbContext UseInMemory(Action<string> logTo = null) {
            var context = CreateDbContext(opt => {
                opt.UseInMemoryDatabase("Mock");
                opt.LogTo(ToFilterLog(logTo));
            });
            context.Seed();
            return context;
        }

        public static TestDbContext UseSqlite(Action<string> logTo = null) {
            var context = CreateDbContext(opt => {
                opt.UseSqlite(CreateInMemoryDatabase());
                opt.LogTo(ToFilterLog(logTo));
            });
            context.Seed();
            return context;
        }

        private static DbConnection CreateInMemoryDatabase() {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        private static Action<string> ToFilterLog(Action<string> logTo = null) {
            if (logTo == null) {
                return Console.WriteLine;
            }
            return s => {
                if (s.Contains("started tracking") || s.Contains("Microsoft.EntityFrameworkCore.ChangeTracking")) {
                    return;
                }
                logTo(s);
            };
        }

        public static TestDbContext CreateDbContext(Action<DbContextOptionsBuilder> action) {
            var builder = new DbContextOptionsBuilder<TestDbContext>();
            action(builder);
            return new TestDbContext(builder.Options);
        }

        private void Seed() {
            Database.EnsureCreated();
            if (!Users.Any()) {
                Users.AddRange(CreateMockUsers().ToArray());
                Accounts.AddRange(CreateMockAccount().ToArray());
                Orders.AddRange(CreateMockOrders().ToArray());
                SaveChanges();
            }
        }

        private static IEnumerable<Order> CreateMockOrders(int count = 10) {
            for (int i = 0; i < count; i++) {
                yield return new Order() {
                    // Id = i,
                    Address = new StreetAddress() {
                        Street = $"Street_{i}",
                        City = "TianJin"
                    }
                };
            }
        }

        private static IEnumerable<User> CreateMockUsers(int count = 100) {
            for (int i = 0; i < count; i++) {
                yield return new User() { UserName = $"name_{i}", FullName = $"Full Name {i % 10}", CreatedDate = DateTime.Now.AddDays(-i) };
            }
        }

        private static IEnumerable<Account> CreateMockAccount(int count = 100) {
            var random = new Random();
            for (int i = 0; i < count; i++) {
                var quantity = random.Next(100);
                var unitConst = Math.Round(random.NextDouble() * 10, 2);
                yield return new Account() { Name = $"name_{i}", Quantity = quantity, UnitConst = unitConst, Price = quantity * unitConst };
            }
        }
    }
}
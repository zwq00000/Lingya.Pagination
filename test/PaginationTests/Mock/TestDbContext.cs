using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PaginationTests.Mock {
       public  class TestDbContext:DbContext {

           public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
           {

           }

           public  DbSet<User> Users { get; set; }
    }
}

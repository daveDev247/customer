using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Data
{
    // EF Core's bridge to the database. Exposes each table as a DbSet.

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
        }

    }
}

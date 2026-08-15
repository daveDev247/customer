using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerApi.Data
{
    // EF Core's bridge to the database. Exposes each table as a DbSet.

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }


        // New tables for auth — do not affect existing Customer behavior.
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
            // Enforce uniqueness on username/email at the DB level, not just in the validator.
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // One-to-many: a User has many RefreshTokens.
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId);
        }

    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    // INITIATES THE DATABASE CONTEXT FOR THE APPLICATION
    // INHERITS FROM IdentityDbContext TO HANDLE USER AUTHENTICATION AND ROLES
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ If you want, configure decimal precision here (optional but clear)
            modelBuilder.Entity<SubscriptionTier>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

        }


        // DbSet for UserTasks to represent the UserTask table in the database
        // public DbSet<UserTask> UserTasks { get; set; }

        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);

        //     // Define One-to-Many Relationship
        //     modelBuilder.Entity<UserTask>()
        //         .HasOne(ut => ut.User)
        //         .WithMany(u => u.UserTasks)
        //         .HasForeignKey(ut => ut.UserId);
        // }
    }
}
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Pomelo.EntityFrameworkCore.MySql;
using Airbnb_Clone_Api.Models;
namespace Airbnb_Clone_Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Define your database tables (DbSets)
        public DbSet<User> Users { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        //public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // You can configure entity relationships here if needed
            modelBuilder.Entity<Listing>()
               .HasOne(l => l.Host) // A Listing has one Host
               .WithMany() // A Host can have many Listings
               .HasForeignKey(l => l.HostId) // Foreign key in Listing table
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Booking>()
             .Property(b => b.Status)
             .HasConversion<string>(); // ✅ Store enum as string instead of integer


            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName).HasColumnType("VARCHAR(100)");
                entity.Property(e => e.LastName).HasColumnType("VARCHAR(100)");
                entity.Property(e => e.Username).HasColumnType("VARCHAR(100)");
                entity.Property(e => e.Email).HasColumnType("VARCHAR(255)");
                entity.Property(e => e.PasswordHash).HasColumnType("TEXT");
                entity.Property(e => e.UserType).HasColumnType("VARCHAR(50)");
            });


            base.OnModelCreating(modelBuilder);

        }


    }
}

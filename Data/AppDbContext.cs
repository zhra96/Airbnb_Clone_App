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
            base.OnModelCreating(modelBuilder);
            // You can configure entity relationships here if needed
            modelBuilder.Entity<Listing>()
               .HasOne(l => l.Host) // A Listing has one Host
               .WithMany() // A Host can have many Listings
               .HasForeignKey(l => l.HostId) // Foreign key in Listing table
               .OnDelete(DeleteBehavior.Cascade);
        }

        //public void Seed()
        //{
        //    if (!Users.Any()) // Avoid duplicate seeding
        //    {
        //        Users.Add(new User { Name = "TestUser", Email = "test@example.com", Role = "Host" });
        //        SaveChanges();
        //    }
        //}


    }
}

using Microsoft.EntityFrameworkCore;
using MvcShopApp.Models;
using System;
using System.Linq;

namespace MvcShopApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Parameterless constructor for EF Core tools
        public ApplicationDbContext() { }

        public DbSet<CatalogItem> CatalogItems { get; set; }
        public DbSet<Coupon> Coupons { get; set; } // Added DbSet for coupons

        // Seed the database with initial catalog items
        public void SeedDatabase()
        {
            if (!CatalogItems.Any())
            {
                CatalogItems.AddRange(new CatalogItem[]
                {
                    new CatalogItem { Name = "Laptop", Price = 9.99m, Description = "A powerful laptop for work and play", ImageUrl = "laptop.png" },
                    new CatalogItem { Name = "Smartphone", Price = 50.99m, Description = "A smartphone with the best camera", ImageUrl = "smartphone.png" },
                    new CatalogItem { Name = "Headphones", Price = 1.99m, Description = "Noise-cancelling headphones for music lovers", ImageUrl = "headphones.png" }
                });

                SaveChanges();
            }

            // Seed the database with initial coupon
            if (!Coupons.Any())
            {
                Coupons.Add(new Coupon
                {
                    Code = "EXAMPLECODE",
                    DiscountType = "percentage",
                    DiscountValue = 10.00m,
                    ExpirationDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc) // Set DateTime to UTC
                });
                Coupons.Add(new Coupon
                {
                    Code = "10OFF",
                    DiscountType = "fixed",
                    DiscountValue = 10.00m,
                    ExpirationDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc) // Set DateTime to UTC
                });

                SaveChanges();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the unique constraint for the Code property in the coupons table
            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();
        }
    }
}

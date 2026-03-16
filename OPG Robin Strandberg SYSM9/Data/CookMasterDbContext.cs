using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OPG_Robin_Strandberg_SYSM9.Models;

namespace OPG_Robin_Strandberg_SYSM9.Data
{
    public class CookMasterDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CookMaster");

            Directory.CreateDirectory(folder);

            optionsBuilder.UseSqlite($"Data Source={Path.Combine(folder, "cookmaster.db")}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TPH: User and AdminUser share one table, distinguished by the "UserType" discriminator column
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<AdminUser>("Admin");

            // Ingredients are stored as a JSON string in the database
            modelBuilder.Entity<Recipe>()
                .Property(r => r.Ingredients)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                    v => JsonSerializer.Deserialize<List<string>>(v, JsonSerializerOptions.Default)
                         ?? new List<string>()
                );

            // Recipe -> User (CreatedBy) one-to-many with shadow FK "CreatedById"
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.CreatedBy)
                .WithMany(u => u.RecipeList)
                .HasForeignKey("CreatedById")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Models;
using System.Reflection.Emit;

namespace RaktarKeszlet.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet-ek a modellekhez
        public DbSet<Company> Companies { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<StorageContainer> StorageContainers { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<TransactionLog> TransactionLogs { get; set; }  

        public DbSet<Category> Categories { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Kikapcsoljuk a kaszkádolt törlést a Termék hierarchia-kapcsolatainál
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Company)
                .WithMany()
                .HasForeignKey(p => p.CompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Building)
                .WithMany()
                .HasForeignKey(p => p.BuildingId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Room)
                .WithMany()
                .HasForeignKey(p => p.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Shelf)
                .WithMany()
                .HasForeignKey(p => p.ShelfId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.StorageContainer)
                .WithMany()
                .HasForeignKey(p => p.StorageContainerId)
                .OnDelete(DeleteBehavior.NoAction);
            // Megmondjuk az EF-nek, hogy a Cégek és a Felhasználók kapcsolata NEM 1:1,
            // így az index nem lehet egyedi (IsUnique = false).
            modelBuilder.Entity<Company>()
                .HasIndex(c => c.UserId)
                .IsUnique(false);




        }

    }
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RaktarKeszlet.Models;

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

    }
}

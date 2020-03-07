using Microsoft.EntityFrameworkCore;
using PocketBookServer.Models;

namespace PocketBookServer.Data
{
    public class ApplicationDataContext : DbContext
    {
        public ApplicationDataContext()
        {
        }

        public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : base(options)
        {
        }

        public DbSet<Medication> Medications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Medication>()
                .HasIndex(m => m.Name)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
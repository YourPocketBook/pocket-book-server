using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PocketBookServer.Models;

namespace PocketBookServer.Data
{
    public class ApplicationDataContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDataContext()
        {
        }

        public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : base(options)
        {
        }

        public DbSet<Medication> Medications { get; set; }
    }
}
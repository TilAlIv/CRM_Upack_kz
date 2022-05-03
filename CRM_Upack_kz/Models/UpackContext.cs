using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace CRM_Upack_kz.Models
{
    public class UpackContext : IdentityDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Application> Applications { get; set; }

        public UpackContext(DbContextOptions options) : base(options)
        {
            
        }
        
        
   }
}
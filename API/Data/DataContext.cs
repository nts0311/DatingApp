using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DbSet<AppUser> User { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}
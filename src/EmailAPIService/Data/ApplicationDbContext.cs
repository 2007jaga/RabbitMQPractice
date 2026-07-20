using EmailAPIService.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailAPIService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FailedEmailMessage> FailedEmailMessages { get; set; }
    }
}
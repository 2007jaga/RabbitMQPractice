using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EmailAPIService.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // optionsBuilder.UseSqlServer(
            //     "Server=JAGANNATH\\SQLEXPRESS;Database=RabbitMQPracticeDB;Trusted_Connection=True;TrustServerCertificate=True;");

            optionsBuilder.UseSqlServer(
                      "Server=JAGANNATH;Database=RabbitMQPracticeDB;User Id=sa;Password=P@ssword12345;TrustServerCertificate=True;"
            );
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
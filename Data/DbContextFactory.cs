using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TestDashboard.Data
{
    public class DbContextFactory
    {
        private readonly IConfiguration _config;

        public DbContextFactory(IConfiguration config)
        {
            _config = config;
        }

        public ApplicationDbContext Create(string client)
        {
            string connKey = client switch
            {
                "BestPet" => "BestPet",
                "Capital" => "Capital",
                "Danya B" => "DanyaB",
                "Test-1" => "DefaultConnection",
                _ => "DefaultConnection"  // fallback
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_config.GetConnectionString(connKey))
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}

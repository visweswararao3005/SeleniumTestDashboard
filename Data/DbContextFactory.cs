using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace TestDashboard.Data
{
    public class DbContextFactory
    {
        private readonly IConfiguration _config;

        public DbContextFactory(IConfiguration config)
        {
            _config = config;
        }

        public ApplicationDbContext Create(string connectionString)
        {
            
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}

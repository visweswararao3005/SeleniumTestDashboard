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

        public ApplicationDbContext Create(string client)
        {
            if (string.IsNullOrEmpty(client))
                throw new ArgumentNullException(nameof(client));

            // Try to get connection string from appsettings.json
            var conn = _config.GetConnectionString(client);

            if (string.IsNullOrWhiteSpace(conn))
            {
                // Throw KeyNotFound so controller can catch and handle it gracefully
                throw new KeyNotFoundException($"Client '{client}' is not configured.");
            }

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(conn)
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}

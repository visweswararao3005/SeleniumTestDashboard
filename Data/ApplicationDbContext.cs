using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TestDashboard.Models;

namespace TestDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }
        public DbSet<TestRunResult> TestRunResults { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TestDashboard.Models;

namespace TestDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }
        public DbSet<TestRunResult> TestRunResults { get; set; }
        public DbSet<ClientInfo> ClientInfo { get; set; }
        public DbSet<TestInfo> TestInfo { get; set; }
        public DbSet<ScheduleModel> TestSchedules { get; set; }
        public DbSet<ClientTestCases> ClientTestCases { get; set; }

    }
}

using TestDashboard.Data;
using TestDashboard.Models;

namespace TestDashboard.Helper
{
    public class ClientsHelper
    {
        private readonly ApplicationDbContext _context;
        public ClientsHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<string> GetClients()
        {
            return _context.ClientInfo
                .OrderBy(r => r.Id)
                .Select(r => r.ClientName)
                .ToList();
        }

        public string GetClientConnectionString(string client)
        {
            try
            {
                var connString = _context.ClientInfo
                    .Where(c => c.ClientName == client)
                    .Select(c => c.ClientConnectionString)
                    .FirstOrDefault();

                return string.IsNullOrWhiteSpace(connString) ? null : connString;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Dictionary<string, string> GetTestNames()
        {
            return _context.TestInfo
                .OrderBy(r => r.Id)
                .ToDictionary(r => r.Id.ToString(), r => r.TestNames);
        }


        public void AddScheduler(ScheduleModel data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Add the schedule to the DbSet
            _context.TestSchedules.Add(data);

            // Save changes to the database
            _context.SaveChanges();

        }
    }
}

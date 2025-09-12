using System;

namespace TestDashboard.Models
{
    public class TestRunResult
    {
        public int Id { get; set; }
        public string TestID { get; set; }
        public string TestName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationSeconds { get; set; }
        public string Status { get; set; }
        public DateTime RunDate { get; set; }
        public string ClientName { get; set; }
        public string Screen { get; set; }
    }
}

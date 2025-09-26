namespace TestDashboard.Models
{
    // Model for schedule
    public class ScheduleModel
    {
        public int Id { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public string? ClientName { get; set; }
        public string? TestsToBeRun { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? LastRunTime { get; set; }
        public string? DaysOfWeek { get; set; }
        public string? AtTime { get; set; }
        public bool IsActive { get; set; }

        public bool All { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using TestDashboard.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TestDashboard.Helper;
using TestDashboard.Models;  // ✅ Needed for KeyNotFoundException

[Route("api/[controller]")]
[ApiController]
public class DashboardApiController : ControllerBase
{
    private readonly DbContextFactory _factory;
    private readonly ClientsHelper _clientsHelper;
    private readonly IConfiguration _config;
    public DashboardApiController(DbContextFactory factory, ClientsHelper clientsHelper, IConfiguration config)
    {
        _factory = factory;
        _clientsHelper = clientsHelper;
        _config = config;
    }

    // Pie: Pass vs Fail counts (today or range)
    [HttpGet("status-summary")]
    public async Task<IActionResult> StatusSummary(string client, string date = null)
    {
        try
        {
            string connectionString = _clientsHelper.GetClientConnectionString(client);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return NotFound(new { error = $"Configuration for '{client}' is not complete, please contact OPAL Support." });
            }
            using var _db = _factory.Create(connectionString);
            var target = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);
            var q = _db.TestRunResults.Where(r => r.RunDate == target.Date);
            var pass = await q.CountAsync(r => r.Status == "Pass" && r.ClientName == client);
            var fail = await q.CountAsync(r => r.Status == "Fail" && r.ClientName == client);
            return Ok(new { pass, fail });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    //overall status summary
    [HttpGet("overall-status-summary")]
    public async Task<IActionResult> OverallStatusSummary(string client)
    {
        try
        {
            string connectionString = _clientsHelper.GetClientConnectionString(client);
            if (string.IsNullOrEmpty(connectionString))
                throw new KeyNotFoundException($"Configuration for '{client}' is not complete, please contact OPAL Support.");
            using var _db = _factory.Create(connectionString);
            var q = _db.TestRunResults;
            var pass = await q.CountAsync(r => r.Status == "Pass" && r.ClientName == client);
            var fail = await q.CountAsync(r => r.Status == "Fail" && r.ClientName == client);
            return Ok(new { pass, fail });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // Bar: Tests per run (grouped by TestName) for a date
    [HttpGet("tests-per-run")]
    public async Task<IActionResult> TestsPerRun(string client, string date = null)
    {
        try
        {
            string connectionString = _clientsHelper.GetClientConnectionString(client);
            if (string.IsNullOrEmpty(connectionString))
                throw new KeyNotFoundException($"Configuration for '{client}' is not complete, please contact OPAL Support.");
            using var _db = _factory.Create(connectionString);

            var target = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);
            var data = await _db.TestRunResults
                .Where(r => r.RunDate == target.Date && r.ClientName == client)
                .GroupBy(r => r.TestName)
                .Select(g => new { TestName = g.Key, Count = g.Count(), Pass = g.Count(x => x.Status == "Pass") })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
            return Ok(data);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // Line: Execution trend (average duration per day for last N days)
    [HttpGet("duration-trend")]
    public async Task<IActionResult> DurationTrend(string client, int days = 14)
    {
        try
        {
            string connectionString = _clientsHelper.GetClientConnectionString(client);
            if (string.IsNullOrEmpty(connectionString))
                throw new KeyNotFoundException($"Configuration for '{client}' is not complete, please contact OPAL Support.");
            using var _db = _factory.Create(connectionString);

            var start = DateTime.Today.AddDays(-days + 1);

            var data = await _db.TestRunResults
                .Where(r => r.RunDate >= start && r.ClientName == client)
                .GroupBy(r => r.RunDate)
                .Select(g => new
                {
                    Date = g.Key,
                    AvgDuration = g.Average(x => x.DurationSeconds),
                    TotalDuration = g.Sum(x => x.DurationSeconds),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(data);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // Recent: Latest RunID test cases
    [HttpGet("recent-runs")]
    public async Task<IActionResult> GetRecentRuns(string client, [FromQuery] DateTime? date = null)
    {
        try
        {
            string connectionString = _clientsHelper.GetClientConnectionString(client);
            if (string.IsNullOrEmpty(connectionString))
                throw new KeyNotFoundException($"Configuration for '{client}' is not complete, please contact OPAL Support.");
            using var _db = _factory.Create(connectionString);

            var latestRunId = await _db.TestRunResults
                .Where(t => t.ClientName == client)
                .OrderByDescending(t => t.StartTime)
                .Select(t => t.TestID)
                .FirstOrDefaultAsync();

            var runs = await _db.TestRunResults
                .Where(t => t.TestID == latestRunId && t.ClientName == client)
                .OrderByDescending(t => t.StartTime)
                .Select(t => new
                {
                    t.TestID,
                    t.TestName,
                    t.StartTime,
                    t.EndTime,
                    t.Screen,
                    DurationSeconds = EF.Functions.DateDiffSecond(t.StartTime, t.EndTime),
                    t.Status
                })
                .ToListAsync();

            return Ok(runs);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    // ✅ New: Test Case History by date (and optional TestID)
    [HttpGet("test-history")]
    public async Task<IActionResult> GetTestHistory(string client, [FromQuery] string date, [FromQuery] string testId = null, [FromQuery] string screen = null)
    {
        try
        {
            string connectionString = _clientsHelper.GetClientConnectionString(client);
            if (string.IsNullOrEmpty(connectionString))
                throw new KeyNotFoundException($"Configuration for '{client}' is not complete, please contact OPAL Support.");
            using var _db = _factory.Create(connectionString);
            var target = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);

            var query = _db.TestRunResults
                .Where(r => r.RunDate == target.Date && r.ClientName == client);

            if (!string.IsNullOrEmpty(testId))
            {
                query = query.Where(r => r.TestID == testId);
            }
            if (!string.IsNullOrEmpty(screen))
            {
                query = query.Where(r => r.Screen == screen);
            }

            var data = await query
                .Where(r => r.ClientName == client)
                .OrderByDescending(r => r.StartTime)
                .Select(r => new
                {
                    r.TestID,
                    r.TestName,
                    r.StartTime,
                    r.EndTime,
                    r.Screen,
                    Duration = EF.Functions.DateDiffSecond(r.StartTime, r.EndTime),
                    r.Status
                })
                .ToListAsync();

            return Ok(data);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }



    [HttpPost("SaveSchedule")]
    public IActionResult SaveSchedule([FromBody] ScheduleDto model)
    {
        if (string.IsNullOrWhiteSpace(model.ClientName))
            return BadRequest(new { success = false, message = "Client name required" });

        string connectionString = _config.GetConnectionString("DefaultConnection");
        using var _db = _factory.Create(connectionString);

        ScheduleModel entity;

        if (model.Id > 0) // 🔹 Update existing
        {
            entity = _db.TestSchedules.FirstOrDefault(s => s.Id == model.Id);
            if (entity == null)
                return NotFound(new { success = false, message = "Schedule not found" });
        }
        else // 🔹 New insert
        {
            entity = new ScheduleModel();
            _db.TestSchedules.Add(entity);
        }
        DateTime? toDate = model.ToDate;
        if (model.ToDate != null)
        {
            // Add 1 day and subtract 1 second to get 23:59:59 of the same day
            toDate = model.ToDate.Value.Date.AddDays(1).AddSeconds(-1);
        }
        entity.CreatedDateTime = DateTime.Now;
        entity.ClientName = model.ClientName;
        entity.TestsToBeRun = string.Join(",", model.TestsToBeRun);
        entity.FromDate = model.FromDate;
        entity.ToDate = toDate;
        entity.DaysOfWeek = model.DayOfWeek.Count != 0 ? string.Join(",", model.DayOfWeek) : null;
        entity.AtTime = model.AtTime;
        entity.IsActive = true;

        _db.SaveChanges();
        return Ok(new { success = true, message = "Schedule saved successfully" });
    }
    public class ScheduleDto
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public List<string> TestsToBeRun { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<string> DayOfWeek { get; set; }
        public string AtTime { get; set; }
    }

    [HttpGet("GetSchedulesByClient")]
    public IActionResult GetSchedulesByClient(string clientName)
    {

        if (string.IsNullOrWhiteSpace(clientName))
            return BadRequest("Client name required");

        string connectionString = _config.GetConnectionString("DefaultConnection");

        using var _db = _factory.Create(connectionString);
        var schedules = _db.TestSchedules
                     .Where(s => s.ClientName == clientName && s.IsActive == true)
                     .OrderByDescending(s => s.Id)
                     .Select(s => new
                     {
                        id = s.Id,
                        clientName = s.ClientName ?? string.Empty,
                        testsToBeRun = s.TestsToBeRun ?? string.Empty,
                        fromDate = s.FromDate,
                        toDate = s.ToDate,
                        daysOfWeek = s.DaysOfWeek ?? "ALL",
                        atTime = s.AtTime ?? string.Empty,
                        lastRunTime = s.LastRunTime
                     })
                     .ToList();
        return Ok(schedules);
    }

    [HttpGet("GetTestsByClient")]
    public IActionResult GetTestsByClient(string clientName)
    {
        if (string.IsNullOrWhiteSpace(clientName))
            return BadRequest("Client name required");

        string connectionString = _config.GetConnectionString("DefaultConnection");
        using var _db = _factory.Create(connectionString);
        // fetch tests from your DB (example)
        var tests = _db.ClientTestCases
            .Where(t => t.ClientName == clientName)
            .Select(t => new { testCaseName = t.TestCaseName })
            .ToList();

        return Ok(tests);
    }

    [HttpPost("DeactivateSchedule")]
    public IActionResult DeactivateSchedule(int id)
    {
        string connectionString = _config.GetConnectionString("DefaultConnection");
        using var _db = _factory.Create(connectionString);

        var entity = _db.TestSchedules.FirstOrDefault(s => s.Id == id);
        if (entity == null)
            return NotFound(new { success = false, message = "Schedule not found" });

        entity.IsActive = false;
        _db.SaveChanges();

        return Ok(new { success = true, message = "Schedule deactivated successfully" });
    }

}

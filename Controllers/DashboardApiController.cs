using Microsoft.AspNetCore.Mvc;
using TestDashboard.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class DashboardApiController : ControllerBase
{
    private readonly DbContextFactory _factory;

    public DashboardApiController(DbContextFactory factory)
    {
        _factory = factory;
    }
    // Pie: Pass vs Fail counts (today or range)
    [HttpGet("status-summary")]
    public async Task<IActionResult> StatusSummary(string client, string date = null )
    {
        using var _db = _factory.Create(client);
        if(string.IsNullOrEmpty(client))
            client = "Test-1";

        var target = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);
        var q = _db.TestRunResults.Where(r => r.RunDate == target.Date);
        var pass = await q.CountAsync(r => r.Status == "Pass" && r.ClientName == client);
        var fail = await q.CountAsync(r => r.Status == "Fail" && r.ClientName == client);
        return Ok(new { pass, fail });
    }

    //overall status summary
    [HttpGet("overall-status-summary")]
    public async Task<IActionResult> OverallStatusSummary(string client)
    {
        using var _db = _factory.Create(client);
        if (string.IsNullOrEmpty(client))
            client = "Test-1";

        var q = _db.TestRunResults;
        var pass = await q.CountAsync(r => r.Status == "Pass" && r.ClientName == client);
        var fail = await q.CountAsync(r => r.Status == "Fail" && r.ClientName == client);
        return Ok(new { pass, fail });
    }

    // Bar: Tests per run (grouped by TestName) for a date
    [HttpGet("tests-per-run")]
    public async Task<IActionResult> TestsPerRun(string client,string date = null)
    {
        using var _db = _factory.Create(client);
        if (string.IsNullOrEmpty(client))
            client = "Test-1";

        var target = string.IsNullOrEmpty(date) ? DateTime.Today : DateTime.Parse(date);
        var data = await _db.TestRunResults
            .Where(r => r.RunDate == target.Date && r.ClientName == client)
            .GroupBy(r => r.TestName)
            .Select(g => new { TestName = g.Key, Count = g.Count(), Pass = g.Count(x => x.Status == "Pass") })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        return Ok(data);
    }

    // Line: Execution trend (average duration per day for last N days)
    [HttpGet("duration-trend")]
    public async Task<IActionResult> DurationTrend(string client, int days = 14)
    {
        using var _db = _factory.Create(client);
        if (string.IsNullOrEmpty(client))
            client = "Test-1";

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

    // Recent: Latest RunID test cases
    [HttpGet("recent-runs")]
    public async Task<IActionResult> GetRecentRuns(string client, [FromQuery] DateTime? date = null)
    {
        using var _db = _factory.Create(client);
        if (string.IsNullOrEmpty(client))
            client = "Test-1";

        // find the latest RunID
        var latestRunId = await _db.TestRunResults
            .Where(t => t.ClientName == client)
            .OrderByDescending(t => t.StartTime)
            .Select(t => t.TestID )
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

    // ✅ New: Test Case History by date (and optional TestID)
    [HttpGet("test-history")]
    public async Task<IActionResult> GetTestHistory(string client, [FromQuery] string date, [FromQuery] string testId = null, [FromQuery] string screen = null)
    {
        using var _db = _factory.Create(client);
        if (string.IsNullOrEmpty(client))
            client = "Test-1";

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
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;
using SocietyApp.Models;

public class CheckController : Controller
{
    private readonly AppDbContext _context;
    public CheckController(AppDbContext context) => _context = context;

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    public IActionResult Index(string phone)
    {
        var member = _context.Members
            .Include(m => m.Contributions)
            .FirstOrDefault(m => m.Phone == phone && !m.IsDeleted);

        if (member == null)
        {
            ViewBag.Error = "Phone number not found.";
            return View();
        }

        // normalize start date (first day of start month)
        var start = new DateTime(member.StartDate.Year, member.StartDate.Month, 1);

        // current month (first day)
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);

        // number of months inclusive
        int totalMonths = (currentMonthStart.Year - start.Year) * 12 +
                          (currentMonthStart.Month - start.Month) + 1;
        if (totalMonths < 0) totalMonths = 0;

        decimal monthly = member.MonthlyAmount;

        // build month list
        var months = new List<(DateTime MonthStart, string Label)>();
        for (int i = 0; i < totalMonths; i++)
        {
            var m = start.AddMonths(i);
            months.Add((m, m.ToString("yyyy-MM")));
        }

        // contributions (non-deleted only)
        var contributions = member.Contributions
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Date)
            .Select(c => new { Id = c.Id, Date = c.Date, Remaining = c.Amount })
            .ToList();

        decimal totalGiven = contributions.Sum(c => c.Remaining);

        // Allocate contributions FIFO to months
        var monthRows = new List<MonthRow>();
        int contribIndex = 0;
        decimal contribRemaining = contributions.Count > 0 ? contributions[0].Remaining : 0m;

        foreach (var (monthStart, label) in months)
        {
            decimal due = monthly;
            decimal paidApplied = 0m;

            while (due > 0 && contribIndex < contributions.Count)
            {
                decimal take = Math.Min(contribRemaining, due);
                paidApplied += take;
                due -= take;
                contribRemaining -= take;

                if (contribRemaining <= 0.0000001m)
                {
                    contribIndex++;
                    if (contribIndex < contributions.Count)
                        contribRemaining = contributions[contribIndex].Remaining;
                }
            }

            string status;
            if (paidApplied >= monthly) status = "Paid";
            else if (paidApplied > 0) status = "Partial";
            else status = "Due";

            monthRows.Add(new MonthRow
            {
                MonthStart = monthStart,
                Label = label,
                Due = monthly,
                Paid = paidApplied,
                Status = status
            });
        }

        // totals
        var totalDue = monthRows.Sum(r => r.Due);
        var totalPaid = totalGiven;
        var pendingTotal = Math.Max(totalDue - totalPaid, 0m);

        // ✅ society-wide data (ignore soft-deleted)
        var societyExpenses = _context.Expenses
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.Date)
            .ToList();

        var totalSocietyGet = _context.Contributions
            .Where(c => !c.IsDeleted)
            .Sum(c => c.Amount);

        var totalSocietySpent = _context.Expenses
            .Where(e => !e.IsDeleted)
            .Sum(e => e.Amount);

        var balance = totalSocietyGet - totalSocietySpent;
        var personContributions = member.Contributions
    .Where(c => !c.IsDeleted)
    .OrderByDescending(c => c.Date)
    .ToList();
        // pass to view
        ViewBag.MonthRows = monthRows;
        ViewBag.TotalDue = totalDue;
        ViewBag.TotalPaid = totalPaid;
        ViewBag.Pending = pendingTotal;

        ViewBag.SocietyExpenses = societyExpenses;
        ViewBag.SocietyTotalGet = totalSocietyGet;
        ViewBag.SocietySpent = totalSocietySpent;
        ViewBag.SocietyBalance = balance;
        ViewBag.TotalMonths = months.Count;
        ViewBag.PersonContributions = personContributions;
        return View("Result", member);
    }

    private class MonthRow
    {
        public DateTime MonthStart { get; set; }
        public string Label { get; set; } = "";
        public decimal Due { get; set; }
        public decimal Paid { get; set; }
        public string Status { get; set; } = "";
    }
}

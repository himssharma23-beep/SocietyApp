using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;
using SocietyApp.Models;

namespace SocietyApp.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AdminController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    private bool IsAuthorized() =>
        HttpContext.Session.GetString("Admin") == "true";

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string password)
    {
        if (password == _config["Admin:Password"])
        {
            HttpContext.Session.SetString("Admin", "true");
            return RedirectToAction("Dashboard");
        }
        ViewBag.Error = "Invalid password";
        return View();
    }

    public IActionResult Dashboard()
    {
        if (!IsAuthorized()) return RedirectToAction("Login");
        ViewBag.Members = _context.Members.Where(m => !m.IsDeleted).ToList();
        ViewBag.Expenses = _context.Expenses.Where(e => !e.IsDeleted).OrderByDescending(e => e.Date).ToList();
        ViewBag.Contributions = _context.Contributions
                                       .Where(c => !c.IsDeleted)
                                       .Include(c => c.Member)
                                       .OrderByDescending(c => c.Date)
                                       .ToList();

        return View();
    }

    [HttpPost]
    public IActionResult AddMember(string name, string phone, string houseNumber,decimal totalToGive, decimal monthlyAmount, string? startDate)
    {
        DateTime start;
        if (!string.IsNullOrEmpty(startDate) && DateTime.TryParseExact(startDate + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out start))
        {
            // parsed successfully (first day of month)
        }
        else
        {
            // default to first day of current month
            var now = DateTime.UtcNow;
            start = new DateTime(now.Year, now.Month, 1);
        }
        _context.Members.Add(new Member { Name = name, Phone = phone, TotalToGive = totalToGive, HouseNumber= houseNumber, MonthlyAmount = monthlyAmount, StartDate= start });
        _context.SaveChanges();
        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    public IActionResult AddContribution(int memberId, decimal amount, string? date)
    {
        DateTime contribDate;

        if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsed))
        {
            contribDate = parsed;
        }
        else
        {
            contribDate = DateTime.Now; // fallback if no date provided
        }
        _context.Contributions.Add(new Contribution { MemberId = memberId, Amount = amount, Date = contribDate });
        _context.SaveChanges();
        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    public IActionResult AddExpense(string comment, decimal amount, string? date)
    {
        DateTime expenseDate;

        if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsed))
        {
            expenseDate = parsed;
        }
        else
        {
            expenseDate = DateTime.Now; // fallback if no date provided
        }
        _context.Expenses.Add(new Expense { Comment = comment, Amount = amount, Date = expenseDate });
        _context.SaveChanges();
        return RedirectToAction("Dashboard");
    }
    // --- EDIT MEMBER ---
    [HttpPost]
    public IActionResult EditMember(int id, string name, string phone, string houseNumber, decimal monthlyAmount, string? startDate)
    {
        var member = _context.Members.FirstOrDefault(m => m.Id == id);
        if (member != null)
        {
            member.Name = name;
            member.Phone = phone;
            member.HouseNumber = houseNumber;
            member.MonthlyAmount = monthlyAmount;

            if (!string.IsNullOrEmpty(startDate) &&
                DateTime.TryParseExact(startDate + "-01", "yyyy-MM-dd", null,
                    System.Globalization.DateTimeStyles.None, out DateTime parsed))
            {
                member.StartDate = parsed;
            }

            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }

    // --- EDIT EXPENSE ---
    [HttpPost]
    public IActionResult EditExpense(int id, string comment, decimal amount, string? date)
    {
        var exp = _context.Expenses.FirstOrDefault(e => e.Id == id);
        if (exp != null)
        {
            exp.Comment = comment;
            exp.Amount = amount;
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsed))
            {
                exp.Date = parsed;
            }
            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }

    // --- EDIT CONTRIBUTION ---
    [HttpPost]
    public IActionResult EditContribution(int id, decimal amount, string? date)
    {
        var contrib = _context.Contributions.FirstOrDefault(c => c.Id == id);
        if (contrib != null)
        {
            contrib.Amount = amount;
            // optional: allow editing date if date provided (yyyy-MM-dd)
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var parsed))
            {
                contrib.Date = parsed;
            }
            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }
    // --- DELETE MEMBER ---
    // Soft-delete member (marks member and their contributions as deleted)
    [HttpPost]
    public IActionResult DeleteMember(int id)
    {
        var member = _context.Members
            .Include(m => m.Contributions)
            .FirstOrDefault(m => m.Id == id);

        if (member != null)
        {
            var now = DateTime.UtcNow;
            member.IsDeleted = true;
            member.DeletedAt = now;

            foreach (var c in member.Contributions)
            {
                c.IsDeleted = true;
                c.DeletedAt = now;
            }

            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }

    // Soft-delete contribution
    [HttpPost]
    public IActionResult DeleteContribution(int id)
    {
        var contrib = _context.Contributions.FirstOrDefault(c => c.Id == id);
        if (contrib != null)
        {
            contrib.IsDeleted = true;
            contrib.DeletedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }

    // Soft-delete expense
    [HttpPost]
    public IActionResult DeleteExpense(int id)
    {
        var exp = _context.Expenses.FirstOrDefault(e => e.Id == id);
        if (exp != null)
        {
            exp.IsDeleted = true;
            exp.DeletedAt = DateTime.UtcNow;
            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }

    // Restore member + contributions
    [HttpPost]
    public IActionResult RestoreMember(int id)
    {
        var member = _context.Members
            .Include(m => m.Contributions)
            .FirstOrDefault(m => m.Id == id);

        if (member != null)
        {
            member.IsDeleted = false;
            member.DeletedAt = null;
            foreach (var c in member.Contributions)
            {
                c.IsDeleted = false;
                c.DeletedAt = null;
            }
            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }

    // Restore contribution
    [HttpPost]
    public IActionResult RestoreContribution(int id)
    {
        var c = _context.Contributions.FirstOrDefault(x => x.Id == id);
        if (c != null)
        {
            c.IsDeleted = false;
            c.DeletedAt = null;
            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }

    // Restore expense
    [HttpPost]
    public IActionResult RestoreExpense(int id)
    {
        var e = _context.Expenses.FirstOrDefault(x => x.Id == id);
        if (e != null)
        {
            e.IsDeleted = false;
            e.DeletedAt = null;
            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }

    // Optional: Hard-delete permanently (use carefully)
    [HttpPost]
    public IActionResult HardDeleteExpense(int id)
    {
        var e = _context.Expenses.FirstOrDefault(x => x.Id == id);
        if (e != null)
        {
            _context.Expenses.Remove(e);
            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }



}

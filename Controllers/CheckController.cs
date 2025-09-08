using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;

namespace SocietyApp.Controllers;

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
            .FirstOrDefault(m => m.Phone == phone);

        if (member == null)
        {
            ViewBag.Error = "Phone number not found.";
            return View();
        }

        var totalGiven = member.Contributions.Sum(c => c.Amount);
        var pending = member.TotalToGive - totalGiven;

        var totalSocietyGet = _context.Contributions.Sum(c => c.Amount);
        var totalSocietySpent = _context.Expenses.Sum(e => e.Amount);
        var balance = totalSocietyGet - totalSocietySpent;

        ViewBag.TotalGiven = totalGiven;
        ViewBag.Pending = pending;
        ViewBag.Expenses = _context.Expenses.ToList();
        ViewBag.SocietyTotalGet = totalSocietyGet;
        ViewBag.SocietySpent = totalSocietySpent;
        ViewBag.Balance = balance;

        return View("Result", member);
    }
}

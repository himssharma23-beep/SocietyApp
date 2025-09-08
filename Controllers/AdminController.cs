using Microsoft.AspNetCore.Mvc;
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
        ViewBag.Members = _context.Members.ToList();
        ViewBag.Expenses = _context.Expenses.ToList();
        return View();
    }

    [HttpPost]
    public IActionResult AddMember(string name, string phone, string houseNumber,decimal totalToGive)
    {
        _context.Members.Add(new Member { Name = name, Phone = phone, TotalToGive = totalToGive, HouseNumber= houseNumber });
        _context.SaveChanges();
        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    public IActionResult AddContribution(int memberId, decimal amount)
    {
        _context.Contributions.Add(new Contribution { MemberId = memberId, Amount = amount, Date = DateTime.Now });
        _context.SaveChanges();
        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    public IActionResult AddExpense(string comment, decimal amount)
    {
        _context.Expenses.Add(new Expense { Comment = comment, Amount = amount, Date = DateTime.Now });
        _context.SaveChanges();
        return RedirectToAction("Dashboard");
    }
}

using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add SQLite DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=society.db"));

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Check}/{action=Index}/{id?}");

app.Run();

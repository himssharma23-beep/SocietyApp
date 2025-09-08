using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SocietyApp.Data;
using SocietyApp.Models;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// DataProtection keys persisted (so session cookies survive restarts)
var keysFolder = Path.Combine(AppContext.BaseDirectory, "DataProtection-Keys");
Directory.CreateDirectory(keysFolder);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("SocietyApp");

// DB - place SQLite file in application base directory (writable)
var dbFile = Path.Combine(AppContext.BaseDirectory, "society.db");
var connectionString = $"Data Source={dbFile}";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".SocietyApp.Session";
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
});

var app = builder.Build();

// Apply migrations at startup and fall back to EnsureCreated if migrate fails
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    Console.WriteLine("Database migrated successfully.");
}
catch (Exception ex)
{
    Console.WriteLine("Migrate failed: " + ex);
    try
    {
        using var scope2 = app.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<AppDbContext>();
        db2.Database.EnsureCreated();
        Console.WriteLine("Database created with EnsureCreated().");
    }
    catch (Exception ex2)
    {
        Console.WriteLine("EnsureCreated also failed: " + ex2);
        throw; // fail startup so Render shows the full error
    }
}
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.AdminUsers.Any())
    {
        var hasher = new PasswordHasher<AdminUser>();
        var admin = new AdminUser
        {
            Username = "himanshu",
            IsSuperAdmin = true,
            CanAddMember = true,
            CanAddContribution = true,
            CanAddExpense = true,
            CanView = true,
            CanDelete = true
        };
        admin.PasswordHash = hasher.HashPassword(admin, "ChangeThisPassword123!"); // change after first login
        db.AdminUsers.Add(admin);
        db.SaveChanges();
        Console.WriteLine("Seeded initial admin user: admin");
    }
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(name: "default",
    pattern: "{controller=Check}/{action=Index}/{id?}");

app.Run();

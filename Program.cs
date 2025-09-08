using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using SocietyApp.Data;
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

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(name: "default",
    pattern: "{controller=Check}/{action=Index}/{id?}");

app.Run();

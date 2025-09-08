using Microsoft.EntityFrameworkCore;
using SocietyApp.Models;

namespace SocietyApp.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Member> Members { get; set; }
        public DbSet<Contribution> Contributions { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }

    }
}

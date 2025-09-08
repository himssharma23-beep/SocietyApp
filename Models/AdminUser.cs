namespace SocietyApp.Models;

public class AdminUser
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = ""; // store a hash, not plain text

    // Permission flags
    public bool CanAddMember { get; set; } = false;
    public bool CanAddContribution { get; set; } = false;
    public bool CanAddExpense { get; set; } = false;
    public bool CanView { get; set; } = false;   // can access dashboard
    public bool CanDelete { get; set; } = false; // can soft-delete/hard-delete
    public bool IsSuperAdmin { get; set; } = false; // can manage users & assign perms

    // soft-delete fields (optional)
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}

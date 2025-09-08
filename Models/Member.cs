namespace SocietyApp.Models
{
    public class Member
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Phone { get; set; }
        public required string HouseNumber { get; set; }
        public decimal TotalToGive { get; set; }
        public List<Contribution> Contributions { get; set; } = new();
        public decimal MonthlyAmount { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}

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
    }
}

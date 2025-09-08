namespace SocietyApp.Models
{
    public class Contribution
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public  Member Member { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}

namespace SocietyApp.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public required string Comment { get; set; }
        public decimal Amount { get; set; }
    }
}

namespace SchoolMS.Models
{
    public class Installment
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public int FeeId { get; set; }
        public Fee Fee { get; set; }
        public bool IsPaid { get; internal set; }
        public decimal Amount { get; internal set; }
    }
}

namespace SchoolMS.DTO
{
    public class InstallmentDTO
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public int FeeId { get; set; }
        public FeeDto Fee { get; set; } // If FeeDTO is defined
        public bool IsPaid { get; set; }
        public decimal Amount { get; set; }
    }
}

namespace SchoolMS.DTO
{
    public class FeeDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public int NumberOfInstallments { get; set; }
        public int StudentId { get; set; }
        public decimal AmountPerInstallment { get; set; } // Add this if it's used in calculations
        public decimal RemainingBalance { get; internal set; }
    }
}

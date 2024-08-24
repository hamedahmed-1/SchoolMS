using System.ComponentModel.DataAnnotations;

namespace SchoolMS.Models
{
    public class Fee
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Total Amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Total Amount must be greater than zero.")]
        public decimal TotalAmount { get; set; }
        [Required(ErrorMessage = "Number of Installments is required.")]
        [Range(8, 9, ErrorMessage = "Number of Installments must be either 8 or 9.")]
        public int NumberOfInstallments { get; set; } // 8 or 9 installments
        public decimal AmountPerInstallment { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public ICollection<Installment> Installments { get; set; }
        public decimal RemainingBalance { get; internal set; }
    }
}

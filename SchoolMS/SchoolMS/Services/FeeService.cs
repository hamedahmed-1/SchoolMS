using SchoolMS.Models;

namespace SchoolMS.Services
{
    public class FeeService
    {
        public decimal CalculateInstallmentAmount(decimal totalAmount, int numberOfInstallments)
        {
            return totalAmount / numberOfInstallments;
        }

        public void RecordInstallmentPayment(Fee fee, decimal amountPaid)
        {
            var remainingInstallments = fee.Installments.Where(i => i.RemainingBalance > 0).ToList();

            foreach (var installment in remainingInstallments)
            {
                if (amountPaid <= 0) break;

                if (amountPaid >= installment.RemainingBalance)
                {
                    amountPaid -= installment.RemainingBalance;
                    installment.RemainingBalance = 0;
                    installment.IsPaid = true;
                    installment.PaymentDate = DateTime.UtcNow;
                }
                else
                {
                    installment.RemainingBalance -= amountPaid;
                    amountPaid = 0;
                }
            }

            // Optionally, update the Fee's RemainingBalance here
            fee.RemainingBalance = fee.Installments.Sum(i => i.RemainingBalance);
        }
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolMS.Data;
using SchoolMS.DTO;
using SchoolMS.Models;
using SchoolMS.Services;

namespace SchoolMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstallmentsController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IMapper _mapper;
        public InstallmentsController(SchoolContext context, IMapper mapper, IWhatsAppService whatsAppService)
        {
            _context = context;
            _mapper = mapper;
            _whatsAppService = whatsAppService;
        }

        // Get all installments for a specific fee
        [HttpGet("fee/{feeId}")]
        public async Task<ActionResult<IEnumerable<InstallmentDTO>>> GetInstallmentsByFee(int feeId)
        {
            var fee = await _context.Fees.Include(f => f.Installments).FirstOrDefaultAsync(f => f.Id == feeId);
            if (fee == null)
            {
                return NotFound($"Fee with ID {feeId} not found.");
            }

            var installmentDtos = _mapper.Map<IEnumerable<InstallmentDTO>>(fee.Installments);
            return Ok(installmentDtos);
        }


        // Get a specific installment by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Installment>> GetInstallment(int id)
        {
            var installment = await _context.Installments.Include(i => i.Fee).FirstOrDefaultAsync(i => i.Id == id);

            if (installment == null)
            {
                return NotFound($"Installment with ID {id} not found.");
            }

            return installment;
        }

        // Process payment for an installment
        [HttpPost("pay")]
        public async Task<IActionResult> PayInstallment(int installmentId, decimal amountPaid)
        {
            //var installment = await _context.Installments.Include(i => i.Fee).ThenInclude(f => f.Student).FirstOrDefaultAsync(i => i.Id == installmentId);

            //if (installment == null)
            //{
            //    return NotFound($"Installment with ID {installmentId} not found.");
            //}

            //if (installment.IsPaid)
            //{
            //    return BadRequest("This installment has already been paid.");
            //}

            //if (amountPaid < installment.Amount)
            //{
            //    return BadRequest("Insufficient amount paid. The full installment amount must be paid.");
            //}

            //var fee = installment.Fee;
            //fee.RemainingBalance -= installment.Amount;
            //installment.IsPaid = true;
            //installment.PaymentDate = DateTime.UtcNow;
            //installment.RemainingBalance = fee.RemainingBalance;

            //await _context.SaveChangesAsync();

            var installment = await _context.Installments
                .Include(i => i.Fee)
                .ThenInclude(f => f.Student)
                .FirstOrDefaultAsync(i => i.Id == installmentId);

            if (installment == null)
            {
                return NotFound($"Installment with ID {installmentId} not found.");
            }

            var fee = installment.Fee;

            // Check if the amount paid is less than or equal to the remaining balance
            if (amountPaid <= 0 || amountPaid > fee.RemainingBalance)
            {
                return BadRequest("Invalid payment amount.");
            }

            // Apply payment to the current installment
            decimal originalAmountPaid = amountPaid; // Keep track of the actual amount the user paid
            decimal remainingToPay = amountPaid;
            decimal originalInstallmentAmount = installment.Amount;

            //if (amountPaid >= installment.Amount)
            //{
            //    installment.IsPaid = true;
            //    installment.PaymentDate = DateTime.UtcNow;
            //    amountPaid -= installment.Amount; // Subtract the installment amount from the payment
            //    fee.RemainingBalance -= originalInstallmentAmount;
            //}
            //else
            //{
            //    // Partial payment logic for the installment
            //    installment.Amount -= amountPaid; // Deduct the amount paid from the current installment amount
            //    fee.RemainingBalance -= amountPaid; // Deduct the amount paid from the remaining balance
            //    amountPaid = 0; // All of the paid amount has been applied to this installment
            //}

            //// If there's extra money left, apply it to the next installments
            //var nextInstallments = await _context.Installments
            //    .Where(i => i.FeeId == fee.Id && !i.IsPaid)
            //    .OrderBy(i => i.PaymentDate)
            //    .ToListAsync();

            //foreach (var nextInstallment in nextInstallments)
            //{
            //    if (amountPaid <= 0) break;

            //    if (amountPaid >= nextInstallment.Amount)
            //    {
            //        nextInstallment.IsPaid = true;
            //        nextInstallment.PaymentDate = DateTime.UtcNow;
            //        amountPaid -= nextInstallment.Amount; // Deduct the installment amount from the remaining payment
            //        fee.RemainingBalance -= nextInstallment.Amount; // Deduct from the remaining balance
            //    }
            //    else
            //    {
            //        nextInstallment.Amount -= amountPaid;
            //        fee.RemainingBalance -= amountPaid; // Deduct from the remaining balance
            //        amountPaid = 0; // All of the paid amount has been applied
            //    }
            //}

            // Apply payment to the current installment
            if (remainingToPay >= installment.Amount)
            {
                // Pay off the current installment
                remainingToPay -= installment.Amount;
                installment.IsPaid = true;
                installment.PaymentDate = DateTime.UtcNow;
                installment.Amount = 0; // Fully paid installment has no remaining amount
            }
            else
            {
                // Partial payment for the current installment
                installment.Amount -= remainingToPay; // Deduct from the current installment
                remainingToPay = 0; // No more money left to apply
            }

            // Apply remaining payment to future installments if any amount is still left
            if (remainingToPay > 0)
            {
                var nextInstallments = await _context.Installments
                    .Where(i => i.FeeId == fee.Id && !i.IsPaid)
                    .OrderBy(i => i.PaymentDate)
                    .ToListAsync();

                foreach (var nextInstallment in nextInstallments)
                {
                    if (remainingToPay <= 0) break; // Stop if there is no more amount to apply

                    if (remainingToPay >= nextInstallment.Amount)
                    {
                        // Fully pay the next installment
                        remainingToPay -= nextInstallment.Amount;
                        nextInstallment.IsPaid = true;
                        nextInstallment.PaymentDate = DateTime.UtcNow;
                        nextInstallment.Amount = 0; // Fully paid installment
                    }
                    else
                    {
                        // Partial payment for the next installment
                        nextInstallment.Amount -= remainingToPay;
                        remainingToPay = 0; // All paid, no remaining amount left
                    }
                }
            }

            // Update the remaining balance in the fee
            fee.RemainingBalance -= amountPaid;
            installment.RemainingBalance = fee.RemainingBalance;

            await _context.SaveChangesAsync();

            // Send OTP automatically after payment
            var sendOTPDto = new SendOTPDto
            {
                Name = fee.Student.ParentsName,  // Assuming Fee is linked to Student and Student has ParentsName
                Mobile = fee.Student.PhoneNumber, // Assuming Student has PhoneNumber
                //Amount = installment.Amount.ToString(),
                Amount = originalAmountPaid.ToString(),
                RemainingAmount = installment.RemainingBalance.ToString()
            };

            var language = Request.Headers["language"].ToString();
            if (string.IsNullOrEmpty(language))
            {
                language = "ar"; 
            }
            var components = new List<WhatsAppComponent>
            {
                new WhatsAppComponent
                {
                    type = "body",
                    parameters = new List<TextMessageParameter>
                    {
                        new TextMessageParameter { type = "text", text = sendOTPDto.Name },
                        new TextMessageParameter { type = "text", text = sendOTPDto.Amount },
                        new TextMessageParameter { type = "text", text = sendOTPDto.RemainingAmount }
                    }
                }
            };

            var result = await _whatsAppService.SendMessage(sendOTPDto.Mobile, "send_payment", language, components);

            if (!result)
            {
                throw new Exception("Something went wrong while sending the WhatsApp message.");
            }

            return Ok(new { message = "Installment paid successfully.", RemainingBalance = fee.RemainingBalance });
        }

        // Calculate and create installments for a specific fee
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateInstallments(int feeId)
        {
            var fee = await _context.Fees.Include(f => f.Installments).FirstOrDefaultAsync(f => f.Id == feeId);
            if (fee == null)
            {
                return NotFound($"Fee with ID {feeId} not found.");
            }

            if (fee.Installments.Any())
            {
                return BadRequest("Installments have already been calculated for this fee.");
            }

            var installmentAmount = fee.TotalAmount / fee.NumberOfInstallments;
            for (int i = 1; i <= fee.NumberOfInstallments; i++)
            {
                var installment = new Installment
                {
                    FeeId = fee.Id,
                    Amount = installmentAmount,
                    PaymentDate = DateTime.UtcNow.AddMonths(i), // Example due date logic
                    IsPaid = false
                };
                _context.Installments.Add(installment);
            }

            fee.RemainingBalance = fee.TotalAmount;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Installments calculated and created successfully." });
        }

        // Helper method to check if an installment exists
        private bool InstallmentExists(int id)
        {
            return _context.Installments.Any(e => e.Id == id);
        }
    }
}

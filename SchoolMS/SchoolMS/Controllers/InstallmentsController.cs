using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolMS.Data;
using SchoolMS.DTO;
using SchoolMS.Models;

namespace SchoolMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstallmentsController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly IMapper _mapper;
        public InstallmentsController(SchoolContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
            var installment = await _context.Installments.Include(i => i.Fee).FirstOrDefaultAsync(i => i.Id == installmentId);

            if (installment == null)
            {
                return NotFound($"Installment with ID {installmentId} not found.");
            }

            if (installment.IsPaid)
            {
                return BadRequest("This installment has already been paid.");
            }

            if (amountPaid < installment.Amount)
            {
                return BadRequest("Insufficient amount paid. The full installment amount must be paid.");
            }

            installment.IsPaid = true;
            installment.PaymentDate = DateTime.UtcNow;

            var fee = installment.Fee;
            fee.RemainingBalance -= installment.Amount;

            await _context.SaveChangesAsync();

            // Logic to send a WhatsApp message can be added here

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

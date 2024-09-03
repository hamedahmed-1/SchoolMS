using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolMS.Data;
using SchoolMS.DTO;
using SchoolMS.Models;
using System.Text.Json;

namespace SchoolMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeesController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly IMapper _mapper;

        public FeesController(SchoolContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Get all fees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeeDto>>> GetFees(
            string searchTerm = null,
            decimal? minTotalAmount = null, // Filter by minimum total amount
            decimal? maxTotalAmount = null, // Filter by maximum total amount
            string sortBy = "TotalAmount",  // Default sorting by TotalAmount
            string sortDirection = "asc",   // Default sorting direction is ascending
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.Fees.AsQueryable();

            // Searching
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(f => f.Student.Name.Contains(searchTerm)); // Assuming there is a Description field
            }

            // Filtering by Total Amount
            if (minTotalAmount.HasValue)
            {
                query = query.Where(f => f.TotalAmount >= minTotalAmount.Value);
            }

            if (maxTotalAmount.HasValue)
            {
                query = query.Where(f => f.TotalAmount <= maxTotalAmount.Value);
            }

            // Sorting by Total Amount
            if (sortDirection.ToLower() == "desc")
            {
                query = query.OrderByDescending(f => f.TotalAmount);
            }
            else
            {
                query = query.OrderBy(f => f.TotalAmount);
            }

            // Pagination
            var totalItems = await query.CountAsync();
            var fees = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var feeDtos = _mapper.Map<IEnumerable<FeeDto>>(fees);

            // Add pagination metadata
            var paginationMetadata = new
            {
                totalCount = totalItems,
                pageSize,
                currentPage = pageNumber,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(feeDtos);
        }

        // Get a specific fee by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<FeeDto>> GetFee(int id)
        {
            //var fee = await _context.Fees.Include(f => f.Student).Include(f => f.Installments)
            //               .FirstOrDefaultAsync(f => f.Id == id);

            //if (fee == null)
            //{
            //    return NotFound($"Fee with ID {id} not found.");
            //}

            //return fee;
            var fee = await _context.Fees.FindAsync(id);

            if (fee == null)
            {
                return NotFound($"Fee with ID {id} not found.");
            }

            var feeDto = _mapper.Map<FeeDto>(fee);
            return Ok(feeDto);
        }

        // Create a new fee
        [HttpPost]
        public async Task<ActionResult<FeeDto>> CreateFee(FeeDto feeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var fee = _mapper.Map<Fee>(feeDto);
            fee.AmountPerInstallment = fee.TotalAmount / fee.NumberOfInstallments;
            _context.Fees.Add(fee);
            await _context.SaveChangesAsync();

            var createdFeeDto = _mapper.Map<FeeDto>(fee);
            return CreatedAtAction(nameof(GetFee), new { id = fee.Id }, createdFeeDto);
        }

        // Update an existing fee
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFee(int id, FeeDto feeDto)
        {
            if (id != feeDto.Id)
            {
                return BadRequest("Fee ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingFee = await _context.Fees.FindAsync(id);
            if (existingFee == null)
            {
                return NotFound($"Fee with ID {id} not found.");
            }

            //existingFee.TotalAmount = feeDto.TotalAmount;
            //existingFee.NumberOfInstallments = feeDto.NumberOfInstallments;
            //existingFee.AmountPerInstallment = feeDto.TotalAmount / feeDto.NumberOfInstallments;

            // Update other properties as needed

            _mapper.Map(feeDto, existingFee);
            existingFee.AmountPerInstallment = existingFee.TotalAmount / existingFee.NumberOfInstallments;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // Delete a fee
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFee(int id)
        {
            var fee = await _context.Fees.FindAsync(id);
            if (fee == null)
            {
                return NotFound($"Fee with ID {id} not found.");
            }

            _context.Fees.Remove(fee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper method to check if a fee exists
        private bool FeeExists(int id)
        {
            return _context.Fees.Any(e => e.Id == id);
        }
    }
}

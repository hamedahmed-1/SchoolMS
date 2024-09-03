using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolMS.Data;
using SchoolMS.DTO;
using SchoolMS.Models;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SchoolMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly IMapper _mapper;

        public StudentsController(SchoolContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentDTO>>> GetStudents(
            string searchTerm = null,
            int? gradeId = null,
            string sortBy = "Name",         // Default sorting by Name
            string sortDirection = "asc",   // Default sorting direction is ascending
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.Students
            .Include(s => s.Grade)
            .Include(s => s.Fees)
            .ThenInclude(f => f.Installments)
            .AsQueryable();

            // Searching
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.Name.Contains(searchTerm));
            }

            // Filtering
            if (gradeId.HasValue)
            {
                query = query.Where(s => s.GradeId == gradeId.Value);
            }

            // Sorting
            if (sortDirection.ToLower() == "desc")
            {
                query = query.OrderByDescending(s => s.Name);
            }
            else
            {
                query = query.OrderBy(s => s.Name);
            }

            // Pagination
            var totalItems = await query.CountAsync();
            var students = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var studentDtos = _mapper.Map<IEnumerable<StudentDTO>>(students);

            // Add pagination metadata
            var paginationMetadata = new
            {
                totalCount = totalItems,
                pageSize,
                currentPage = pageNumber,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(studentDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDTO>> GetStudent(int id)
        {
            var student = await _context.Students
            .Include(s => s.Grade)
            .Include(s => s.Fees)
            .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            var studentDto = _mapper.Map<StudentDTO>(student);
            return Ok(studentDto);
        }

        [HttpPost]
        public async Task<ActionResult<StudentDTO>> CreateStudent(StudentDTO studentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var student = _mapper.Map<Student>(studentDto);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var students = await _context.Students
                .Include(s => s.Fees) // Eager load the fees
                .ToListAsync();

            var fee = new Fee
            {
                TotalAmount = studentDto.Fee.TotalAmount,
                NumberOfInstallments = studentDto.Fee.NumberOfInstallments,
                AmountPerInstallment = studentDto.Fee.TotalAmount / studentDto.Fee.NumberOfInstallments,
                StudentId = student.Id,
                RemainingBalance = studentDto.Fee.TotalAmount
            };

            _context.Fees.Add(fee);
            await _context.SaveChangesAsync();

            for (int i = 0; i < fee.NumberOfInstallments; i++)
            {
                var installment = new Installment
                {
                    FeeId = fee.Id,
                    Amount = fee.AmountPerInstallment
                };

                _context.Installments.Add(installment);
            }

            await _context.SaveChangesAsync();

            var createdStudentDto = _mapper.Map<StudentDTO>(student);
            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, createdStudentDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, StudentDTO studentDto)
        {
            if (id != studentDto.Id)
            {
                return BadRequest("Student ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the student exists in the database
            var existingStudent = await _context.Students.FindAsync(id);
            if (existingStudent == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            // Update the student details
            existingStudent.Name = studentDto.Name;
            existingStudent.DateOfBirth = studentDto.DateOfBirth;
            existingStudent.GradeId = studentDto.GradeId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound($"Student with ID {id} not found.");
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content on success
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }

    }
}

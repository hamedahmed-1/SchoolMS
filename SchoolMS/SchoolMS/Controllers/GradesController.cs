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
    public class GradesController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly IMapper _mapper;

        public GradesController(SchoolContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Get all grades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GradeDto>>> GetGrades()
        {
            var grades = await _context.Grades
                .Include(g => g.EducationalStage)
                .ToListAsync();

            var gradeDtos = _mapper.Map<IEnumerable<GradeDto>>(grades);

            return Ok(gradeDtos);
        }

        // Get a specific grade by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<GradeDto>> GetGrade(int id)
        {
            var grade = await _context.Grades
                .Include(g => g.EducationalStage)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null)
            {
                return NotFound($"Grade with ID {id} not found.");
            }

            var gradeDto = _mapper.Map<GradeDto>(grade);

            return Ok(gradeDto);
        }

        // Create a new grade
        [HttpPost]
        public async Task<ActionResult<GradeDto>> CreateGrade(GradeDto gradeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensure the EducationalStage exists
            var educationalStage = await _context.EducationalStages.FindAsync(gradeDto.EducationalStageId);
            if (educationalStage == null)
            {
                return BadRequest("Invalid EducationalStageId.");
            }

            var grade = _mapper.Map<Grade>(gradeDto);
            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            var createdGradeDto = _mapper.Map<GradeDto>(grade);

            return CreatedAtAction(nameof(GetGrade), new { id = grade.Id }, createdGradeDto);
        }

        // Update an existing grade
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGrade(int id, GradeDto gradeDto)
        {
            if (id != gradeDto.Id)
            {
                return BadRequest("Grade ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingGrade = await _context.Grades
                .Include(g => g.EducationalStage)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (existingGrade == null)
            {
                return NotFound($"Grade with ID {id} not found.");
            }

            // Check if the EducationalStageId is valid
            if (gradeDto.EducationalStageId != existingGrade.EducationalStageId)
            {
                var educationalStage = await _context.EducationalStages.FindAsync(gradeDto.EducationalStageId);
                if (educationalStage == null)
                {
                    return BadRequest("Invalid EducationalStageId.");
                }
            }

            _mapper.Map(gradeDto, existingGrade);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GradeExists(id))
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

        // Delete a grade
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var grade = await _context.Grades
                .Include(g => g.EducationalStage)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null)
            {
                return NotFound($"Grade with ID {id} not found.");
            }

            _context.Grades.Remove(grade);

            // Optionally, delete the associated educational stage if it has no other grades
            if (!await _context.Grades.AnyAsync(g => g.EducationalStageId == grade.EducationalStageId))
            {
                var educationalStage = grade.EducationalStage;
                _context.EducationalStages.Remove(educationalStage);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper method to check if a grade exists
        private bool GradeExists(int id)
        {
            return _context.Grades.Any(e => e.Id == id);
        }
    }
}

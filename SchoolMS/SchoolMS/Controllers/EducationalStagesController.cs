using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
    public class EducationalStagesController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly IMapper _mapper;

        public EducationalStagesController(SchoolContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/EducationalStages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EducationalStageDto>>> GetEducationalStages(
            string searchTerm = null,
            string sortBy = "Name",         // Default sorting by Name
            string sortDirection = "asc",   // Default sorting direction is ascending
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.EducationalStages
                .Include(e => e.Grades)
                .AsQueryable();

            // Searching
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => e.Name.Contains(searchTerm));
            }

            // Sorting
            if (sortDirection.ToLower() == "desc")
            {
                query = query.OrderByDescending(e => e.Name);
            }
            else
            {
                query = query.OrderBy(e => e.Name);
            }

            // Pagination
            var totalItems = await query.CountAsync();
            var educationalStages = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var educationalStageDtos = _mapper.Map<IEnumerable<EducationalStageDto>>(educationalStages);

            // Add pagination metadata
            var paginationMetadata = new
            {
                totalCount = totalItems,
                pageSize,
                currentPage = pageNumber,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(educationalStageDtos);
        }

        // GET: api/EducationalStages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EducationalStageDto>> GetEducationalStage(int id)
        {
            var educationalStage = await _context.EducationalStages
                .Include(e => e.Grades)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (educationalStage == null)
            {
                return NotFound($"Educational Stage with ID {id} not found.");
            }

            var educationalStageDto = _mapper.Map<EducationalStageDto>(educationalStage);

            return Ok(educationalStageDto);
        }

        // POST: api/EducationalStages
        [HttpPost]
        public async Task<ActionResult<EducationalStageDto>> CreateEducationalStage(EducationalStageDto educationalStageDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var educationalStage = _mapper.Map<EducationalStage>(educationalStageDto);

            _context.EducationalStages.Add(educationalStage);
            await _context.SaveChangesAsync();

            var createdEducationalStageDto = _mapper.Map<EducationalStageDto>(educationalStage);

            return CreatedAtAction(nameof(GetEducationalStage), new { id = educationalStage.Id }, createdEducationalStageDto);
        }

        // PUT: api/EducationalStages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEducationalStage(int id, EducationalStageDto educationalStageDto)
        {
            if (id != educationalStageDto.Id)
            {
                return BadRequest("Educational Stage ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingEducationalStage = await _context.EducationalStages.FindAsync(id);
            if (existingEducationalStage == null)
            {
                return NotFound($"Educational Stage with ID {id} not found.");
            }

            _mapper.Map(educationalStageDto, existingEducationalStage);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EducationalStageExists(id))
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

        // DELETE: api/EducationalStages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEducationalStage(int id)
        {
            var educationalStage = await _context.EducationalStages.FindAsync(id);
            if (educationalStage == null)
            {
                return NotFound($"Educational Stage with ID {id} not found.");
            }

            _context.EducationalStages.Remove(educationalStage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EducationalStageExists(int id)
        {
            return _context.EducationalStages.Any(e => e.Id == id);
        }
    }
}

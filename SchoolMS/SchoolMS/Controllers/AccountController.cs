using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolMS.Data;
using SchoolMS.DTO;
using SchoolMS.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace SchoolMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SchoolContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers(string searchTerm = null,
            string sortBy = "UserName",  // Sort by UserName by default
            string sortDirection = "asc", // Sort ascending by default
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = userManager.Users
                .Select(u => new UserDTO
                {
                    UserName = u.UserName,
                })
                .AsQueryable();

            // Searching
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.UserName.Contains(searchTerm));
            }

            // Sorting
            if (sortDirection.ToLower() == "desc")
            {
                query = query.OrderByDescending(u => u.UserName);
            }
            else
            {
                query = query.OrderBy(u => u.UserName);
            }

            var totalItems = await query.CountAsync();
            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Add pagination metadata
            var paginationMetadata = new
            {
                totalCount = totalItems,
                pageSize,
                currentPage = pageNumber,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(users);
        }

        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration, SchoolContext context)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Registration(RegisterUserDTO userDTO)
        {
            if (ModelState.IsValid)
            {
                //save
                ApplicationUser user = new ApplicationUser();
                user.UserName = userDTO.UserName;
                IdentityResult result = await userManager.CreateAsync(user, userDTO.Password);
                if (result.Succeeded)
                {
                    return Ok("Account Add Success");
                }
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(errors);
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDTO userDTO)
        {
            if (ModelState.IsValid == true)
            {
                //chech - create token
                ApplicationUser user = await userManager.FindByNameAsync(userDTO.UserName);
                if (user != null)
                {
                    bool found = await userManager.CheckPasswordAsync(user, userDTO.Password);
                    if (found)
                    {
                        //Claims Token
                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                        //Get Role
                        var roles = await userManager.GetRolesAsync(user);
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

                        SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                        //Create Token
                        JwtSecurityToken mytoken = new JwtSecurityToken(
                            issuer: configuration["JWT:ValidIssuer"], //URL Web API
                            audience: configuration["JWT:ValidAudiance"], //URL Consumer REACT
                            claims: claims,
                            expires: DateTime.Now.AddHours(1),
                            signingCredentials: signingCredentials
                            );
                        return Ok(new
                        {
                            token= new JwtSecurityTokenHandler().WriteToken(mytoken),
                            expiration = mytoken.ValidTo,
                            username = user.UserName
                        });
                    }
                    return Unauthorized();
                }
                return Unauthorized();
            }
            return Unauthorized();
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // There is no actual token invalidation server-side since JWT is stateless.
            // Clients should handle logout by deleting the token on their side (e.g., from localStorage).

            return Ok(new { message = "Logged out successfully." });
        }
    }
}

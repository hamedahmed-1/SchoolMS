using Microsoft.AspNetCore.Identity;

namespace SchoolMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsAdmin { get; set; }
    }
}

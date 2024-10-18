using System.ComponentModel.DataAnnotations;

namespace SchoolMS.DTO
{
    public class ChangePasswordDTO
    {
        [Required]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}

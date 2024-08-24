using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace SchoolMS.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime DateOfBirth { get; set; }
        [Required(ErrorMessage = "Grade is required.")]
        public int GradeId { get; set; }
        public Grade Grade { get; set; }
        public ICollection<Fee> Fees { get; set; }
    }
}

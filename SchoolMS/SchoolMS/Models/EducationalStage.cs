using System.ComponentModel.DataAnnotations;

namespace SchoolMS.Models
{
    public class EducationalStage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(255)]

        // Navigation Property
        public ICollection<Grade> Grades { get; set; }
    }
}

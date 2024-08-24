using System.ComponentModel.DataAnnotations;

namespace SchoolMS.Models
{
    public class Grade
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        [Required]
        public int EducationalStageId { get; set; } // Foreign key

        // Navigation properties
        public EducationalStage EducationalStage { get; set; }
        public ICollection<Student> Students { get; set; }
    }
}

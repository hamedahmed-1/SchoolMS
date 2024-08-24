using SchoolMS.Models;

namespace SchoolMS.DTO
{
    public class StudentDTO
    {
        public int Id { get; set; } // Typically auto-generated
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int GradeId { get; set; } // Should be set by the client
    }
}

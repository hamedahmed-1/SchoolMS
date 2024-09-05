using SchoolMS.Models;

namespace SchoolMS.DTO
{
    public class StudentDTO
    {
        public int Id { get; set; } // Typically auto-generated

        public string Name { get; set; }

        public string ParentsName { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int GradeId { get; set; } // Should be set by the client

        public FeeDto Fee { get; set; }

    }
}

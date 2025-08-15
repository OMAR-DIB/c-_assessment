using System.ComponentModel.DataAnnotations;

namespace StudentManagementSystem.API.Models.RequestModels
{
    public class UpdateStudentRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Email { get; set; }

        [Range(1, 150)]
        public int Age { get; set; }

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public long SchoolID { get; set; }
    }
}
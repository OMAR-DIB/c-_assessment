namespace StudentManagementSystem.API.Models.RequestModels
{
    public class CreateStudentRequest
    {
        public long ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int Age { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public long SchoolID { get; set; }
    }
}

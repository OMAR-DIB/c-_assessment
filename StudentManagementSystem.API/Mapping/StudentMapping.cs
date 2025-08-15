using StudentManagementSystem.Data.Entities;
using StudentManagementSystem.API.Models.RequestModels;
using StudentManagementSystem.API.Models.ResponseModels;

namespace StudentManagementSystem.API.Mapping
{
    public static class StudentMapping
    {
        public static Student ToEntity(CreateStudentRequest request)
        {
            return new Student
            {
                ID = request.ID,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Age = request.Age,
                DateOfBirth = request.DateOfBirth,
                SchoolID = request.SchoolID,
                CreatedAt = DateTime.Now,
                IsDeleted = false
            };
        }

        public static StudentResponse ToResponse(Student student)
        {
            return new StudentResponse
            {
                ID = student.ID,
                Name = student.Name,
                PhoneNumber = student.PhoneNumber,
                Email = student.Email,
                Age = student.Age,
                DateOfBirth = student.DateOfBirth,
                SchoolID = student.SchoolID
            };
        }
    }
}

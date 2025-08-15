using StudentManagementSystem.API.Models.RequestModels;
using StudentManagementSystem.API.Models.ResponseModels;
using StudentManagementSystem.Data.Entities;

namespace StudentManagementSystem.API.Services
{
    public interface IStudentService
    {
        List<Student> GetAll(long? schoolId = null);
        Student? GetById(long id);
        Student Create(Student student);
        Student Update(Student student);
        bool Delete(long id);
    }
}
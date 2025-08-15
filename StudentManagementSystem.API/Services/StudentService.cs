using StudentManagementSystem.Data.Entities;
using StudentManagementSystem.Data.Repository;

namespace StudentManagementSystem.API.Services
{
    public class StudentService : IStudentService
    {
        private readonly IRepository<Student> _repository;

        public StudentService(IRepository<Student> repository)
        {
            _repository = repository;
        }

        public List<Student> GetAll(long? schoolId = null)
        {
            var students = _repository.GetAll();
            if (schoolId.HasValue)
                students = students.Where(s => s.SchoolID == schoolId.Value).ToList();
            return students;
        }

        public Student? GetById(long id) => _repository.GetByID(id);

        public Student Create(Student student) => _repository.Create(student);

        public Student Update(Student student) => _repository.Update(student);

        public bool Delete(long id) => _repository.Delete(id);
    }
}

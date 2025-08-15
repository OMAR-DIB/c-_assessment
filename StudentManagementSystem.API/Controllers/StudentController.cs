using Microsoft.AspNetCore.Mvc;
using NLog;
using StudentManagementSystem.API.Mapping;
using StudentManagementSystem.API.Models.RequestModels;
using StudentManagementSystem.API.Services;
using StudentManagementSystem.Data.Entities;
using StudentManagementSystem.Data.Repository;

namespace StudentManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IRepository<School> _schoolRepository;
        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public StudentController(IStudentService studentService, IRepository<School> schoolRepository)
        {
            _studentService = studentService;
            _schoolRepository = schoolRepository;
        }

        // GET: api/student?schoolID=1
        [HttpGet]
        public IActionResult GetAll([FromQuery] long? schoolID)
        {
            try
            {
                var students = _studentService.GetAll(schoolID);
                var response = students.Select(StudentMapping.ToResponse).ToList();
                return Ok(response);
            }
            catch (Exception ex)
            {
                //Logger.Error(ex, "Error in GetAll");
                return StatusCode(500, ex.Message);
            }
        }

        // GET: api/student/5
        [HttpGet("{id}")]
        public IActionResult GetById(long id)
        {
            try
            {
                var student = _studentService.GetById(id);
                if (student == null)
                    return NotFound("Student not found");

                return Ok(StudentMapping.ToResponse(student));
            }
            catch (Exception ex)
            {
                //Logger.Error(ex, $"Error in GetById({id})");
                return StatusCode(500, ex.Message);
            }
        }

        // POST: api/student
        [HttpPost]
        [HttpPost]
        public IActionResult Create([FromBody] CreateStudentRequest request)
        {
            try
            {
                // No school existence check — SchoolID is just a field on Student now
                var student = StudentMapping.ToEntity(request);
                var created = _studentService.Create(student);

                return Ok(StudentMapping.ToResponse(created));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // PUT: api/student/5
        // PUT: api/student/5
        [HttpPut("{id}")]
        public IActionResult Update(long id, [FromBody] CreateStudentRequest request)
        {
            try
            {
                var existingStudent = _studentService.GetById(id);
                if (existingStudent == null)
                    return NotFound("Student not found");

                // No school existence check — just update the SchoolID value
                existingStudent.Name = request.Name;
                existingStudent.PhoneNumber = request.PhoneNumber;
                existingStudent.Email = request.Email;
                existingStudent.Age = request.Age;
                existingStudent.DateOfBirth = request.DateOfBirth;
                existingStudent.SchoolID = request.SchoolID;
                existingStudent.UpdatedAt = DateTime.UtcNow;

                var updated = _studentService.Update(existingStudent);

                return Ok(StudentMapping.ToResponse(updated));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE: api/student/5
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            try
            {
                var existingStudent = _studentService.GetById(id);
                if (existingStudent == null)
                    return NotFound("Student not found");

                var deleted = _studentService.Delete(id);
                return Ok(new { Success = deleted });
            }
            catch (Exception ex)
            {
                //Logger.Error(ex, $"Error in Delete({id})");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
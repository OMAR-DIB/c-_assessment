using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StudentManagementSystem.Data.Entities
{
    public class Student : BaseEntity
    { 
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email {  get; set; }
        public int Age { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public long SchoolID { get; set; }
    }
}

namespace StudentManagementSystem.Data.Entities
{
    public class BaseEntity
    {
        public long ID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}

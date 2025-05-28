namespace TaskCreationService.Models
{
    public class TaskEntity
    {
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
    }
}

namespace TaskDashboardService.Models
{
    public class TaskDashboardManagementEntity
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime AssignedDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}

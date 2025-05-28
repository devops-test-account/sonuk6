namespace TaskAssignmentService.Model
{
    public class TaskAssignmentManagementEntity
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}

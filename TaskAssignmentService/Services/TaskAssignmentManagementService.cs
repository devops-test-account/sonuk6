using TaskAssignmentService.Data;
using TaskAssignmentService.Model;

namespace TaskAssignmentService.Services
{

    public class TaskAssignmentManagementService
    {
        private readonly TaskAssignmentManagementDbContext _context;
        private readonly MessagePublisher _publisher;

        public TaskAssignmentManagementService(TaskAssignmentManagementDbContext context, MessagePublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        public void AssignTask(TaskAssignmentManagementEntity taskAssignment)
        {
            _context.TaskAssignments.Add(taskAssignment);
            _context.SaveChanges();
            _publisher.Publish(taskAssignment); // Publish to RabbitMQ
        }

        public TaskAssignmentManagementEntity GetTaskAssignmentById(int id)
        {
            return _context.TaskAssignments.Find(id);
        }
    }
}

using TaskDashboardService.Data;
using TaskDashboardService.Models;

namespace TaskDashboardService.Services
{
    public class TaskDashboardManagementService
    {
        private readonly TaskDashboardManagementDbContext _context;

        public TaskDashboardManagementService(TaskDashboardManagementDbContext context)
        {
            _context = context;
        }

        public TaskDashboardManagementEntity GetTaskById(int id)
        {
            return _context.TaskDashBoard.Find(id);
        }

        public void UpdateTaskStatus(TaskDashboardManagementEntity taskDashboardManagementEntity)
        {
            _context.TaskDashBoard.Add(taskDashboardManagementEntity);
            _context.SaveChanges();
            //var task = _context.TaskDashBoard.Find(id);
            //if (task != null)
            //{
            //    task.Status = status;
            //    _context.SaveChanges();
            //}
        }
    }
}

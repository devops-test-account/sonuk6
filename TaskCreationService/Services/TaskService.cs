using TaskCreationService.Data;
using TaskCreationService.Models;

namespace TaskCreationService.Services
{
    public class TaskService
    {
        private readonly TaskDbContext _context;

        public TaskService(TaskDbContext context)
        {
            _context = context;
        }

        public void CreateTask(TaskEntity task)
        {
            _context.Tasks.Add(task);
            _context.SaveChanges();
        }

        public TaskEntity GetTaskById(int id)
        {
            return _context.Tasks.Find(id);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using TaskAssignmentService.Model;

namespace TaskAssignmentService.Data
{
    public class TaskAssignmentManagementDbContext : DbContext
    {
        public TaskAssignmentManagementDbContext(DbContextOptions<TaskAssignmentManagementDbContext> options) : base(options) { }

        public DbSet<TaskAssignmentManagementEntity> TaskAssignments { get; set; }
    }
}

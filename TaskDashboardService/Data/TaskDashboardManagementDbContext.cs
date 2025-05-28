using Microsoft.EntityFrameworkCore;
using TaskDashboardService.Models;

namespace TaskDashboardService.Data
{
    public class TaskDashboardManagementDbContext : DbContext
    {
        public TaskDashboardManagementDbContext(DbContextOptions<TaskDashboardManagementDbContext> options) : base(options) { }

        public DbSet<TaskDashboardManagementEntity> TaskDashBoard { get; set; }
    }
}

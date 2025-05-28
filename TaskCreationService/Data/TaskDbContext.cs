using Microsoft.EntityFrameworkCore;
using TaskCreationService.Models;

namespace TaskCreationService.Data
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options) { }

        public DbSet<TaskEntity> Tasks { get; set; }
    }

}

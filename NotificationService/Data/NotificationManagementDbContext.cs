using Microsoft.EntityFrameworkCore;
using NotificationService.Models;
using System.Collections.Generic;

namespace NotificationService.Data
{
    public class NotificationManagementDbContext : DbContext
    {
        public NotificationManagementDbContext(DbContextOptions<NotificationManagementDbContext> options) : base(options) { }

        public DbSet<NotificationManagementEntity> Notifications { get; set; }
    }
}

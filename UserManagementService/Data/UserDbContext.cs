using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UserManagementService.Models;

namespace UserManagementService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
    }
}

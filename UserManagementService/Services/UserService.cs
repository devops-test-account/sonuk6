using UserManagementService.Data;
using UserManagementService.Models;

namespace UserManagementService.Services
{
    public class UserService
    {
        private readonly UserDbContext _context;

        public UserService(UserDbContext context)
        {
            _context = context;
        }

        public void CreateUser(UserEntity user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public UserEntity GetUserById(int id)
        {
            return _context.Users.Find(id);
        }

        public UserEntity? GetUserByUsernameAndPassword(string username, string password)
        {
            return _context.Users.FirstOrDefault(u => u.UserName == username && u.Password == password);
        }

    }
}

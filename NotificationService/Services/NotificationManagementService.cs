using NotificationService.Data;
using NotificationService.Models;

namespace NotificationService.Services
{
    public class NotificationManagementService
    {
        private readonly NotificationManagementDbContext _context;

        public NotificationManagementService(NotificationManagementDbContext context)
        {
            _context = context;
        }

        public void SendNotification(NotificationManagementEntity notification)
        {
            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        public NotificationManagementEntity GetNotificationById(int id)
        {
            return _context.Notifications.Find(id);
        }
    }
}

namespace NotificationService.Models
{
    public class NotificationManagementEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public DateTime SentDate { get; set; }
    }
}

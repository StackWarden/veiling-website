namespace backend.Repositories;

public interface INotificationRepository
{
    NotificationEntity GetNotificationById(Guid id);
    IEnumerable<NotificationEntity> GetNotificationsByUserId(Guid userId);
    void AddNotification(NotificationEntity notification);
    void UpdateNotification(NotificationEntity notification);
    void DeleteNotification(Guid id);
}
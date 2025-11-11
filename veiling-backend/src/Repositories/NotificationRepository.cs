namespace backend.Repositories;

using backend.Db.Entities;
using backend.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

public class NotificationRepository : INotificationRepository
{
    private readonly List<NotificationEntity> _notifications = new List<NotificationEntity>();

    public void Add(NotificationEntity notification)
    {
        _notifications.Add(notification);
    }

    public NotificationEntity? GetById(Guid id)
    {
        return _notifications.FirstOrDefault(n => n.Id == id);
    }

    public IEnumerable<NotificationEntity> GetByUserId(Guid userId)
    {
        return _notifications.Where(n => n.UserId == userId).ToList();
    }

    public void Update(NotificationEntity notification)
    {
        var existingNotification = GetById(notification.Id);
        if (existingNotification != null)
        {
            existingNotification.Title = notification.Title;
            existingNotification.Message = notification.Message;
            existingNotification.ReadAt = notification.ReadAt;
        }
    }

    public void Delete(Guid id)
    {
        var notification = GetById(id);
        if (notification != null)
        {
            _notifications.Remove(notification);
        }
    }
}
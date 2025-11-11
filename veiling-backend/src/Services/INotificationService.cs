namespace backend.Services;

using backend.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface INotificationService
{
    Task<NotificationViewModel> CreateNotificationAsync(NotificationViewModel notification);
    Task<NotificationViewModel> GetNotificationByIdAsync(Guid id);
    Task<IEnumerable<NotificationViewModel>> GetAllNotificationsAsync(Guid userId);
    Task<NotificationViewModel> UpdateNotificationAsync(NotificationViewModel notification);
    Task<bool> DeleteNotificationAsync(Guid id);
}
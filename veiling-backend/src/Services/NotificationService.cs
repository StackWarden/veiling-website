namespace backend.Services;

using backend.Models;
using backend.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<NotificationViewModel> CreateNotificationAsync(NotificationViewModel notificationViewModel)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = notificationViewModel.UserId,
            Type = notificationViewModel.Type,
            Title = notificationViewModel.Title,
            Message = notificationViewModel.Message,
            CreatedAt = DateTime.UtcNow,
            ReadAt = null
        };

        await _notificationRepository.AddAsync(notification);
        return notificationViewModel;
    }

    public async Task<IEnumerable<NotificationViewModel>> GetNotificationsByUserIdAsync(Guid userId)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId);
        var notificationViewModels = new List<NotificationViewModel>();

        foreach (var notification in notifications)
        {
            notificationViewModels.Add(new NotificationViewModel
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            });
        }

        return notificationViewModels;
    }

    public async Task<NotificationViewModel> MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification != null)
        {
            notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);
            return new NotificationViewModel
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            };
        }

        return null;
    }

    public async Task DeleteNotificationAsync(Guid notificationId)
    {
        await _notificationRepository.DeleteAsync(notificationId);
    }
}
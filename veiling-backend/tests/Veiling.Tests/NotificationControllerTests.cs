using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using backend.Controllers;
using backend.Services;
using backend.ViewModels;

namespace Veiling.Tests
{
    public class NotificationControllerTests
    {
        private readonly NotificationController _controller;
        private readonly Mock<INotificationService> _mockService;

        public NotificationControllerTests()
        {
            _mockService = new Mock<INotificationService>();
            _controller = new NotificationController(_mockService.Object);
        }

        [Fact]
        public async Task GetNotification_ReturnsOkResult_WhenNotificationExists()
        {
            var notificationId = Guid.NewGuid();
            var notification = new NotificationViewModel
            {
                Id = notificationId,
                UserId = Guid.NewGuid(),
                Type = "Info",
                Title = "Test Notification",
                Message = "This is a test notification.",
                CreatedAt = DateTime.UtcNow
            };

            _mockService.Setup(service => service.GetNotificationAsync(notificationId))
                .ReturnsAsync(notification);

            var result = await _controller.GetNotification(notificationId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnNotification = Assert.IsType<NotificationViewModel>(okResult.Value);
            Assert.Equal(notificationId, returnNotification.Id);
        }

        [Fact]
        public async Task CreateNotification_ReturnsCreatedResult_WhenNotificationIsCreated()
        {
            var notification = new NotificationViewModel
            {
                UserId = Guid.NewGuid(),
                Type = "Info",
                Title = "New Notification",
                Message = "This is a new notification.",
                CreatedAt = DateTime.UtcNow
            };

            _mockService.Setup(service => service.CreateNotificationAsync(notification))
                .ReturnsAsync(notification);

            var result = await _controller.CreateNotification(notification);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnNotification = Assert.IsType<NotificationViewModel>(createdResult.Value);
            Assert.Equal(notification.Title, returnNotification.Title);
        }

        [Fact]
        public async Task UpdateNotification_ReturnsNoContent_WhenNotificationIsUpdated()
        {
            var notificationId = Guid.NewGuid();
            var notification = new NotificationViewModel
            {
                Id = notificationId,
                UserId = Guid.NewGuid(),
                Type = "Info",
                Title = "Updated Notification",
                Message = "This notification has been updated.",
                CreatedAt = DateTime.UtcNow
            };

            _mockService.Setup(service => service.UpdateNotificationAsync(notificationId, notification))
                .Returns(Task.CompletedTask);

            var result = await _controller.UpdateNotification(notificationId, notification);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteNotification_ReturnsNoContent_WhenNotificationIsDeleted()
        {
            var notificationId = Guid.NewGuid();

            _mockService.Setup(service => service.DeleteNotificationAsync(notificationId))
                .Returns(Task.CompletedTask);

            var result = await _controller.DeleteNotification(notificationId);

            Assert.IsType<NoContentResult>(result);
        }
    }
}
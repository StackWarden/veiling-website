using Microsoft.AspNetCore.Mvc;
using backend.ViewModels;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public IActionResult CreateNotification([FromBody] NotificationViewModel notificationViewModel)
        {
            if (notificationViewModel == null)
            {
                return BadRequest("Notification data is required.");
            }

            var createdNotification = _notificationService.CreateNotification(notificationViewModel);
            return CreatedAtAction(nameof(GetNotification), new { id = createdNotification.Id }, createdNotification);
        }

        [HttpGet("{id}")]
        public IActionResult GetNotification(Guid id)
        {
            var notification = _notificationService.GetNotificationById(id);
            if (notification == null)
            {
                return NotFound();
            }

            return Ok(notification);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateNotification(Guid id, [FromBody] NotificationViewModel notificationViewModel)
        {
            if (notificationViewModel == null || id != notificationViewModel.Id)
            {
                return BadRequest("Invalid notification data.");
            }

            var updatedNotification = _notificationService.UpdateNotification(notificationViewModel);
            if (updatedNotification == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteNotification(Guid id)
        {
            var result = _notificationService.DeleteNotification(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
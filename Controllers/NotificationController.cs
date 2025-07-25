using Microsoft.AspNetCore.Mvc;

namespace notification_service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationController : ControllerBase
    {
        private static readonly List<Notification> Notifications = new();

        [HttpGet]
        public ActionResult<List<Notification>> GetAll()
        {
            return Notifications;
        }

        [HttpGet("{id}")]
        public ActionResult<Notification> GetById(Guid id)
        {
            var notification = Notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null) return NotFound();
            return notification;
        }

        [HttpPost]
        public ActionResult<Notification> Create(Notification notification)
        {
            Notifications.Add(notification);
            return CreatedAtAction(nameof(GetById), new { id = notification.Id }, notification);
        }

        [HttpPut("{id}/read")]
        public IActionResult MarkAsRead(Guid id)
        {
            var notification = Notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null) return NotFound();
            notification.IsRead = true;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var notification = Notifications.FirstOrDefault(n => n.Id == id);
            if (notification == null) return NotFound();
            Notifications.Remove(notification);
            return NoContent();
        }

        // TODO: Listen to events from RabbitMQ and create notifications
    }
} 
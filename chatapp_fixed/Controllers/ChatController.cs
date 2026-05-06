using Chat_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chat_App.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDBContext context;

        public ChatController(ApplicationDBContext context)
        {
            this.context = context;
        }


        public IActionResult Index()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return RedirectToAction("Login", "Account");

            var users = context.user.Where(u => u.Id != currentUserId).ToList();

            var userViewModels = new List<UserWithLastMessage>();



            foreach (var user in users)
            {
                var lastMessage = context.message
                    .Where(m => (m.SenderId == currentUserId && m.ReceiverId == user.Id) ||
                                (m.SenderId == user.Id && m.ReceiverId == currentUserId))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefault();

                string lastMessageText = null;
                if (lastMessage != null)
                {
                    if (lastMessage.DeletedStatus == "EveryOne")
                    {
                        lastMessageText = "This message was deleted";
                    }
                    else if (lastMessage.DeletedStatus == "Forme" && lastMessage.DeletedForUserId == currentUserId)
                    {
                        lastMessageText = null;
                    }
                    else
                    {
                        lastMessageText = lastMessage.Text;
                    }
                }

                userViewModels.Add(new UserWithLastMessage
                {
                    User = user,
                    LastMessage = lastMessageText,
                    LastMessageTime = lastMessage?.SentAt
                });
            }

            return View(userViewModels);
        }

        public IActionResult Chat(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            var receiver = context.user.FirstOrDefault(u => u.Id == id);
            if (receiver != null)
            {
                ViewBag.ReceiverName = receiver.username;
                ViewBag.IsOnline = receiver.IsOnline;
                ViewBag.LastSeen = receiver.LastSeen;
            }
            ViewBag.Id = id;
            return View();
        }

        public async Task<IActionResult> GetMessages(int receiverId)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Unauthorized();

            var messages = await context.message
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == receiverId) ||
                            (m.SenderId == receiverId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

        var result = messages.Select(m =>
        {
            if (m.DeletedStatus == "EveryOne")
            {
                return new
                {
                    m.MessageId,
                    m.SenderId,
                    Text = "This message was deleted",
                    m.SentAt,
                    FileType = (string)null,
                    FileName = (string)null,
                    m.IsDelivered,
                    m.IsRead
                };
            }
            else if (m.DeletedStatus == "Forme" && m.DeletedForUserId == currentUserId)
            {
                return null;
            }
            else
            {
                return new
                {
                    m.MessageId,
                    m.SenderId,
                    m.Text,
                    m.SentAt,
                    m.FileType,
                    m.FileName,
                    m.IsDelivered,
                    m.IsRead
                };
            }
        }).Where(m => m != null).ToList();

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteForEveryOne(int messageid)
        {
            var message = context.message.FirstOrDefault(m => m.MessageId == messageid);
            if (message == null) return NotFound();
            message.DeletedStatus = "EveryOne";
            message.DeletedForUserId = null;
            context.message.Update(message);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteForMe(int messageid)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null) return Unauthorized();

            var message = context.message.FirstOrDefault(m => m.MessageId == messageid);
            if (message == null) return NotFound();
            message.DeletedStatus = "Forme";
            message.DeletedForUserId = currentUserId;
            context.message.Update(message);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(int senderId, int receiverId, string fileType)
        {
            try
            {
                var file = Request.Form.Files.FirstOrDefault();
                if (file == null) return BadRequest("No file uploaded");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/uploads/{uniqueFileName}";

                var message = new Message
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Text = relativePath,
                    SentAt = DateTime.UtcNow,
                    FileType = fileType,
                    FileName = file.FileName
                };

                context.message.Add(message);
                await context.SaveChangesAsync();

                return Json(new { filePath = relativePath, messageId = message.MessageId, fileName = file.FileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public class UserWithLastMessage
    {
        public UsersList User { get; set; }
        public string LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
    }
}

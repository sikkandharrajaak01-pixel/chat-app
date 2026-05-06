using Chat_App;
using Chat_App.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

public class ChatHub : Hub
{
    private readonly ApplicationDBContext _db;

    public static ConcurrentDictionary<int, string> Users = new();

    public ChatHub(ApplicationDBContext db)
    {
        _db = db;
    }

    public async Task RegisterUser(int userId)
    {
        Users.AddOrUpdate(userId, Context.ConnectionId, (_, _) => Context.ConnectionId);

        // Update user status to online
        var user = await _db.user.FindAsync(userId);
        if (user != null)
        {
            user.IsOnline = true;
            user.LastSeen = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Broadcast user online status to all clients
            await Clients.All.SendAsync("UserStatusChanged", userId, true, DateTime.UtcNow);
        }
    }

    public async Task SendMessage(
        int senderId,
        int receiverId,
        string message)
    {
        var msg = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Text = message,
            SentAt = DateTime.UtcNow
        };

        _db.message.Add(msg);
        await _db.SaveChangesAsync();

        if (Users.TryGetValue(senderId, out var senderConnId))
        {
            await Clients.Client(senderConnId)
                .SendAsync("ReceiveMessage", senderId, message, msg.MessageId);
        }

        if (Users.TryGetValue(receiverId, out var receiverConnId))
        {
            try
            {
                msg.IsDelivered = true;
                await _db.SaveChangesAsync();

                await Clients.Client(receiverConnId)
                    .SendAsync("ReceiveMessage", senderId, message, msg.MessageId);

                if (Users.TryGetValue(senderId, out var updatedSenderConnId))
                {
                    await Clients.Client(updatedSenderConnId)
                        .SendAsync("MessageDelivered", msg.MessageId);
                }
            }
            catch
            {
                Users.TryRemove(receiverId, out _);
            }
        }
    }

    public async Task SendFileMessage(
        int senderId,
        int receiverId,
        string filePath,
        string fileType,
        string fileName)
    {
        var msg = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Text = filePath,
            SentAt = DateTime.UtcNow,
            FileType = fileType,
            FileName = fileName
        };

        _db.message.Add(msg);
        await _db.SaveChangesAsync();

        if (Users.TryGetValue(senderId, out var senderConnId))
        {
            await Clients.Client(senderConnId)
                .SendAsync("ReceiveFileMessage", senderId, filePath, msg.MessageId, fileType, fileName);
        }

        if (Users.TryGetValue(receiverId, out var receiverConnId))
        {
            try
            {
                msg.IsDelivered = true;
                await _db.SaveChangesAsync();

                await Clients.Client(receiverConnId)
                    .SendAsync("ReceiveFileMessage", senderId, filePath, msg.MessageId, fileType, fileName);

                if (Users.TryGetValue(senderId, out var updatedSenderConnId))
                {
                    await Clients.Client(updatedSenderConnId)
                        .SendAsync("MessageDelivered", msg.MessageId);
                }
            }
            catch
            {
                Users.TryRemove(receiverId, out _);
            }
        }
    }

    public async Task MarkMessagesAsRead(int senderId, int receiverId)
    {
        var unreadMessages = _db.message
            .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
            .ToList();

        foreach (var msg in unreadMessages)
        {
            msg.IsRead = true;
        }

        await _db.SaveChangesAsync();

        if (Users.TryGetValue(senderId, out var senderConnId))
        {
            await Clients.Client(senderConnId)
                .SendAsync("MessageRead", receiverId);
        }
    }

    public async Task GetUserStatus(int userId)
    {
        var user = await _db.user.FindAsync(userId);
        if (user != null)
        {
            await Clients.Caller.SendAsync("UserStatusResponse", userId, user.IsOnline, user.LastSeen);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Users.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (user.Key != 0)
        {
            Users.TryRemove(user.Key, out _);

            // Update user status to offline
            var dbUser = await _db.user.FindAsync(user.Key);
            if (dbUser != null)
            {
                dbUser.IsOnline = false;
                dbUser.LastSeen = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                // Broadcast user offline status to all clients
                await Clients.All.SendAsync("UserStatusChanged", user.Key, false, DateTime.UtcNow);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}
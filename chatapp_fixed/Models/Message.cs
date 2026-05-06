namespace Chat_App.Models
{
    public class Message
    {
        public int MessageId { get; set; }

        public int SenderId { get; set; }

        public int ReceiverId { get; set; }

        public string? Text { get; set; }

        public DateTime SentAt { get; set; }

        public string? DeletedStatus { get; set; }

        public int? DeletedForUserId { get; set; }

        public string? FileType { get; set; }

        public string? FileName { get; set; }

        public bool IsDelivered { get; set; } = false;

        public bool IsRead { get; set; } = false;
    }
}
namespace Chat_App.Models
{
    public class UsersList
    {
        public int Id { get; set; }

        public string username { get; set;  }

        public string password { get; set; }

        public DateTime? LastSeen { get; set; }

        public bool IsOnline { get; set; }
    }
}

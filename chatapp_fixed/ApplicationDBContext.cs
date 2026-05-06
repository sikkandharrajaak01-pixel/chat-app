using Chat_App.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat_App
{
    public class ApplicationDBContext : DbContext
    {

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }
        public DbSet<UsersList> user { get; set; }
            public DbSet<Message> message { get; set; }
    }
}

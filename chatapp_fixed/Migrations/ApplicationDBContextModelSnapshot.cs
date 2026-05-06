using System;
using Chat_App;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Chat_App.Migrations
{
    [DbContext(typeof(ApplicationDBContext))]
    partial class ApplicationDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.23")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Chat_App.Models.UsersList", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer");
                NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));
                b.Property<bool>("IsOnline").HasColumnType("boolean");
                b.Property<DateTime?>("LastSeen").HasColumnType("timestamp with time zone");
                b.Property<string>("password").IsRequired().HasColumnType("text");
                b.Property<string>("username").IsRequired().HasColumnType("text");
                b.HasKey("Id");
                b.ToTable("user");
            });

            modelBuilder.Entity("Chat_App.Models.Message", b =>
            {
                b.Property<int>("MessageId")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("integer");
                NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("MessageId"));
                b.Property<string>("DeletedStatus").HasColumnType("text");
                b.Property<int?>("DeletedForUserId").HasColumnType("integer");
                b.Property<string>("FileName").HasColumnType("text");
                b.Property<string>("FileType").HasColumnType("text");
                b.Property<bool>("IsDelivered").HasColumnType("boolean");
                b.Property<bool>("IsRead").HasColumnType("boolean");
                b.Property<int>("ReceiverId").HasColumnType("integer");
                b.Property<int>("SenderId").HasColumnType("integer");
                b.Property<DateTime>("SentAt").HasColumnType("timestamp with time zone");
                b.Property<string>("Text").HasColumnType("text");
                b.HasKey("MessageId");
                b.ToTable("message");
            });
#pragma warning restore 612, 618
        }
    }
}

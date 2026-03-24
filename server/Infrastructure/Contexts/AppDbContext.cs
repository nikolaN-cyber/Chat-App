using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Contexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { 
        }
        public DbSet<User> _users => Set<User>();
        public DbSet<Message> _messages => Set<Message>();
        public DbSet<Conversation> _conversation => Set<Conversation>();
        public DbSet<Participation> _participations => Set<Participation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Participation>(entity =>
            {
                entity.HasKey(p => new { p.UserId, p.ConversationId });

                entity
                    .HasOne(p => p.User)
                    .WithMany(u => u.Participations)
                    .HasForeignKey(p => p.UserId);

                entity
                    .HasOne(p => p.Conversation)
                    .WithMany(c => c.Participants)
                    .HasForeignKey(p => p.ConversationId);
            });

            modelBuilder.Entity<Conversation>()
                .HasOne(c => c.Admin)
                .WithMany(u => u.AdminAt)
                .HasForeignKey(c => c.AdminId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

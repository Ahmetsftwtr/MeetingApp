using MeetingApp.Model.Entities;
using MeetingApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeetingApp.DataAccess.Context
{
    public class MeetingAppDbContext : DbContext
    {
        public MeetingAppDbContext(DbContextOptions<MeetingAppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingDocument> MeetingDocuments { get; set; }
    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);

                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasMany(e => e.Meetings)
                      .WithOne(e => e.User)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Meeting>(entity =>
            {
                entity.ToTable("Meetings");
                entity.HasKey(e => e.Id);
                entity.ToTable(tb => tb.HasTrigger("trg_Meetings_AfterDelete"));

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.IsCancelled);

                entity.HasOne(e => e.User)
                      .WithMany(e => e.Meetings)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Documents)
                      .WithOne(e => e.Meeting)
                      .HasForeignKey(e => e.MeetingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MeetingDocument>(entity =>
            {
                entity.ToTable("MeetingDocuments");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FileExtension).HasMaxLength(10);
                entity.Property(e => e.ContentType).HasMaxLength(100);
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.UploadedAt).IsRequired();

                entity.HasIndex(e => e.MeetingId);
                entity.HasIndex(e => e.UploadedAt);

                entity.HasOne(e => e.Meeting)
                      .WithMany(e => e.Documents)
                      .HasForeignKey(e => e.MeetingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}

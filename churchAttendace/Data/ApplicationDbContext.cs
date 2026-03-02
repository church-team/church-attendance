using churchAttendace.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Stage> Stages => Set<Stage>();
        public DbSet<StageManagerStage> StageManagerStages => Set<StageManagerStage>();
        public DbSet<Class> Classes => Set<Class>();
        public DbSet<ClassServant> ClassServants => Set<ClassServant>();
        public DbSet<Child> Children => Set<Child>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Attendance> Attendance => Set<Attendance>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<Complaint> Complaints => Set<Complaint>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<Stage>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<StageManagerStage>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.StageId })
                    .IsUnique()
                    .HasFilter("[IsActive] = 1");
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StageId);
                entity.HasIndex(e => e.IsActive);

                entity.Property(e => e.AssignedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Stage)
                    .WithMany(s => s.StageManagers)
                    .HasForeignKey(e => e.StageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasIndex(e => e.StageId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.Name, e.StageId })
                    .IsUnique()
                    .HasFilter("[IsActive] = 1");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.Stage)
                    .WithMany(s => s.Classes)
                    .HasForeignKey(e => e.StageId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClassServant>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.ClassId })
                    .IsUnique()
                    .HasFilter("[IsActive] = 1");
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ClassId);
                entity.HasIndex(e => e.IsActive);

                entity.Property(e => e.AssignedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Class)
                    .WithMany(c => c.Servants)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.AssignedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Child>(entity =>
            {
                entity.HasIndex(e => e.ClassId);
                entity.HasIndex(e => e.FullName);
                entity.HasIndex(e => e.ParentPhoneNumber);
                entity.HasIndex(e => e.IsActive);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.Class)
                    .WithMany(c => c.Children)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasIndex(e => e.ClassId);
                entity.HasIndex(e => e.SessionDate);
                entity.HasIndex(e => e.CreatedByUserId);
                entity.HasIndex(e => e.IsActive);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.Class)
                    .WithMany(c => c.Sessions)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany(u => u.CreatedSessions)
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => new { e.SessionId, e.ChildId });
                entity.HasIndex(e => e.ChildId);
                entity.HasIndex(e => new { e.SessionId, e.IsPresent });
                entity.HasIndex(e => e.RecordedByUserId);

                entity.Property(e => e.RecordedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Session)
                    .WithMany(s => s.AttendanceRecords)
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Child)
                    .WithMany()
                    .HasForeignKey(e => e.ChildId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.RecordedByUser)
                    .WithMany(u => u.RecordedAttendances)
                    .HasForeignKey(e => e.RecordedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.TableName);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Action);

                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsActive);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<Complaint>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.RepliedAt);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.RepliedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.RepliedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}

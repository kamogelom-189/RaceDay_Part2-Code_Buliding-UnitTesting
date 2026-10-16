
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class RaceDayContext : DbContext
    {
        public RaceDayContext(
            DbContextOptions<RaceDayContext> options)
            : base(options)
        {
        }

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<AppUser> Users => Set<AppUser>();

        public DbSet<RaceEvent> Events => Set<RaceEvent>();

        public DbSet<Category> Categories => Set<Category>();

        public DbSet<Enrolment> Enrolments => Set<Enrolment>();

        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ROLES
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");

                entity.HasKey(r => r.RoleId);

                entity.Property(r => r.RoleName)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .IsRequired();

                entity.HasIndex(r => r.RoleName)
                    .IsUnique();

                entity.HasData(
                    new Role
                    {
                        RoleId = 1,
                        RoleName = "Organiser"
                    },
                    new Role
                    {
                        RoleId = 2,
                        RoleName = "Participant"
                    });
            });

            // USERS
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(u => u.UserId);

                entity.Property(u => u.FullName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .IsRequired();

                entity.Property(u => u.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .IsRequired();

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.PasswordHash)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .IsRequired();

                entity.Property(u => u.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // EVENTS
            modelBuilder.Entity<RaceEvent>(entity =>
            {
                entity.ToTable("Events", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Events_EventType",
                        "[EventType] IN ('run', 'walk', 'cycle')");
                });

                entity.HasKey(e => e.EventId);

                entity.Property(e => e.Title)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnType("varchar(max)");

                entity.Property(e => e.EventDate)
                    .HasColumnType("datetime");

                entity.Property(e => e.Location)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .IsRequired();

                // Event distance 
                entity.Property(e => e.DistanceKM)
                    .HasColumnType("decimal(6,2)")
                    .IsRequired();

                entity.Property(e => e.EventType)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasDefaultValue("run")
                    .IsRequired();

                entity.HasOne(e => e.Organiser)
                    .WithMany(u => u.OrganisedEvents)
                    .HasForeignKey(e => e.OrganiserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // CATEGORIES
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");

                entity.HasKey(c => c.CategoryId);

                entity.Property(c => c.CategoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .IsRequired();

                entity.Property(c => c.DistanceKM)
                    .HasColumnType("decimal(5,2)");

                entity.Property(c => c.EntryFee)
                    .HasColumnType("decimal(10,2)");

                entity.HasOne(c => c.Event)
                    .WithMany(e => e.Categories)
                    .HasForeignKey(c => c.EventId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ENROLMENTS
            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.ToTable("Enrolments", table =>
                {
                    table.HasCheckConstraint(
                        "CK_Enrolments_PaymentStatus",
                        "[PaymentStatus] IN ('Pending', 'Paid', 'Cancelled')");
                });

                entity.HasKey(e => e.EnrolmentId);

                entity.Property(e => e.EnrolmentDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.PaymentStatus)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("Pending")
                    .IsRequired();

                entity.HasOne(e => e.Participant)
                    .WithMany(u => u.Enrolments)
                    .HasForeignKey(e => e.ParticipantId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Enrolments)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // RESULTS
            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable("Results");

                entity.HasKey(r => r.ResultId);

                entity.Property(r => r.RaceNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .IsRequired();

                entity.Property(r => r.FinishTime)
                    .HasColumnType("time");

                entity.HasIndex(r => r.EnrolmentId)
                    .IsUnique();

                entity.HasOne(r => r.Enrolment)
                    .WithOne(e => e.Result)
                    .HasForeignKey<Result>(r => r.EnrolmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

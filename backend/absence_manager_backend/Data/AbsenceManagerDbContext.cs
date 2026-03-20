using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class AbsenceManagerDbContext : DbContext
    {
        public DbSet<AppUser> AppUsers => Set<AppUser>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Desk> Desks => Set<Desk>();
        public DbSet<Reservation> Reservations => Set<Reservation>();

        public AbsenceManagerDbContext(DbContextOptions<AbsenceManagerDbContext> options) : base(options) { }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var roomId = "11111111-1111-1111-1111-111111111111";

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUsers");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.DisplayName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(320);

                entity.Property(x => x.EntraObjectId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.TenantId)
                    .HasMaxLength(100);

                entity.HasIndex(x => new { x.EntraObjectId, x.TenantId })
                    .IsUnique();
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("Rooms");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();
            });

            modelBuilder.Entity<Desk>(entity =>
            {
                entity.ToTable("Desks");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasOne(x => x.Room)
                    .WithMany(x => x.Desks)
                    .HasForeignKey(x => x.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.RoomId, x.Name })
                    .IsUnique();
            });

            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.ToTable("Reservations");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Date)
                    .IsRequired();

                entity.Property(x => x.CreatedAtUtc)
                    .IsRequired();

                entity.HasOne(x => x.User)
                    .WithMany(x => x.Reservations)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Desk)
                    .WithMany(x => x.Reservations)
                    .HasForeignKey(x => x.DeskId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.DeskId, x.Date })
                    .IsUnique();

                entity.HasIndex(x => new { x.UserId, x.Date })
                    .IsUnique();
            });

            modelBuilder.Entity<Room>().HasData(
                new Room { Id = roomId, Name = "Iroda 1" });



            modelBuilder.Entity<Desk>().HasData(
                new Desk { Id = "aaaaaaaa-0000-0000-0000-000000000001", RoomId = roomId, Name = "A1" },
                new Desk { Id = "aaaaaaaa-0000-0000-0000-000000000002", RoomId = roomId, Name = "A2" },
                new Desk { Id = "aaaaaaaa-0000-0000-0000-000000000003", RoomId = roomId, Name = "A3" },
                new Desk { Id = "aaaaaaaa-0000-0000-0000-000000000004", RoomId = roomId, Name = "A4" },
                new Desk { Id = "aaaaaaaa-0000-0000-0000-000000000005", RoomId = roomId, Name = "A5" },
                new Desk { Id = "aaaaaaaa-0000-0000-0000-000000000006", RoomId = roomId, Name = "A6" }
            );
        }
    }
}

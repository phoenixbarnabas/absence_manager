using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class AbsenceManagerDbContext : DbContext
    {
        public DbSet<AppUser> AppUsers => Set<AppUser>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Office> Offices => Set<Office>();
        public DbSet<Workstation> Workstations => Set<Workstation>();
        public DbSet<OfficeBooking> OfficeBookings => Set<OfficeBooking>();

        public AbsenceManagerDbContext(DbContextOptions<AbsenceManagerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------
            // AppUser
            // -------------------------
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUsers");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.EntraObjectId)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.TenantId)
                    .HasMaxLength(100);

                entity.Property(x => x.DisplayName)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasMaxLength(320);

                entity.Property(x => x.IsActive)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasIndex(x => new { x.EntraObjectId, x.TenantId })
                    .IsUnique()
                    .HasFilter("\"TenantId\" IS NOT NULL");
            });

            // -------------------------
            // Location
            // -------------------------
            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Locations");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .IsRequired();

                entity.Property(x => x.DisplayOrder)
                    .IsRequired();

                entity.HasIndex(x => x.Name)
                    .IsUnique();
            });

            // -------------------------
            // Office
            // -------------------------
            modelBuilder.Entity<Office>(entity =>
            {
                entity.ToTable("Offices");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(500);

                entity.Property(x => x.IsActive)
                    .IsRequired();

                entity.Property(x => x.DisplayOrder)
                    .IsRequired();

                entity.HasOne(x => x.Location)
                    .WithMany(x => x.Offices)
                    .HasForeignKey(x => x.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.LocationId, x.Name })
                    .IsUnique();
            });

            // -------------------------
            // Workstation
            // -------------------------
            modelBuilder.Entity<Workstation>(entity =>
            {
                entity.ToTable("Workstations");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Code)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.IsActive)
                    .IsRequired();

                entity.Property(x => x.DisplayOrder)
                    .IsRequired();

                entity.Property(x => x.PositionX)
                    .HasColumnType("decimal(10,2)");

                entity.Property(x => x.PositionY)
                    .HasColumnType("decimal(10,2)");

                entity.HasOne(x => x.Office)
                    .WithMany(x => x.Workstations)
                    .HasForeignKey(x => x.OfficeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.OfficeId, x.Code })
                    .IsUnique();

                entity.HasIndex(x => new { x.OfficeId, x.Name })
                    .IsUnique();
            });

            // -------------------------
            // OfficeBooking
            // -------------------------
            modelBuilder.Entity<OfficeBooking>(entity =>
            {
                entity.ToTable("OfficeBookings");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.BookingDate)
                    .IsRequired();

                entity.Property(x => x.CreatedAtUtc)
                    .IsRequired();

                entity.Property(x => x.CreatedByUserId)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.CancelledByUserId)
                    .HasMaxLength(50);

                entity.Property(x => x.IsCancelled)
                    .IsRequired();

                entity.HasOne(x => x.Workstation)
                    .WithMany(x => x.Bookings)
                    .HasForeignKey(x => x.WorkstationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.OfficeBookings)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Egy workstation egy adott napra csak egyszer foglalható
                entity.HasIndex(x => new { x.WorkstationId, x.BookingDate, x.IsCancelled })
                    .HasDatabaseName("IX_OfficeBookings_Workstation_BookingDate_IsCancelled");

                // Egy user egy adott napra csak egy aktív foglalással rendelkezhet
                entity.HasIndex(x => new { x.UserId, x.BookingDate, x.IsCancelled })
                    .HasDatabaseName("IX_OfficeBookings_AppUser_BookingDate_IsCancelled");
            });

            // -------------------------
            // Seed data
            // -------------------------
            //modelBuilder.Entity<Location>().HasData(
            //    new Location
            //    {
            //        Id = "1",
            //        Name = "Budapest",
            //        IsActive = true,
            //        DisplayOrder = 1
            //    }
            //);

            //modelBuilder.Entity<Office>().HasData(
            //    new Office
            //    {
            //        Id = "1",
            //        LocationId = "1",
            //        Name = "Iroda 1",
            //        Description = "Alapértelmezett iroda",
            //        IsActive = true,
            //        DisplayOrder = 1
            //    }
            //);

            //modelBuilder.Entity<Workstation>().HasData(
            //    new Workstation { Id = "1", OfficeId = "1", Code = "WS-001", Name = "1. hely", IsActive = true, DisplayOrder = 1 },
            //    new Workstation { Id = "2", OfficeId = "1", Code = "WS-002", Name = "2. hely", IsActive = true, DisplayOrder = 2 },
            //    new Workstation { Id = "3", OfficeId = "1", Code = "WS-003", Name = "3. hely", IsActive = true, DisplayOrder = 3 },
            //    new Workstation { Id = "4", OfficeId = "1", Code = "WS-004", Name = "4. hely", IsActive = true, DisplayOrder = 4 },
            //    new Workstation { Id = "5", OfficeId = "1", Code = "WS-005", Name = "5. hely", IsActive = true, DisplayOrder = 5 },
            //    new Workstation { Id = "6", OfficeId = "1", Code = "WS-006", Name = "6. hely", IsActive = true, DisplayOrder = 6 }
            //);

            var location1 = new Location
            {
                Id = "loc-budapest",
                Name = "Budapest HQ",
                IsActive = true,
                DisplayOrder = 1
            };

            // ===== OFFICE =====
            var office1 = new Office
            {
                Id = "office-bp-1",
                LocationId = location1.Id,
                Name = "Open Office - 1st Floor",
                Description = "Main open office area",
                IsActive = true,
                DisplayOrder = 1
            };

            var office2 = new Office
            {
                Id = "office-bp-2",
                LocationId = location1.Id,
                Name = "Quiet Room",
                Description = "Silent workspace",
                IsActive = true,
                DisplayOrder = 2
            };

            // ===== WORKSTATIONS =====
            var workstations = new List<Workstation>
            {
                new Workstation
                {
                    Id = "ws-1",
                    OfficeId = office1.Id,
                    Code = "A1",
                    Name = "Desk A1",
                    IsActive = true,
                    DisplayOrder = 1,
                    PositionX = 1,
                    PositionY = 1
                },
                new Workstation
                {
                    Id = "ws-2",
                    OfficeId = office1.Id,
                    Code = "A2",
                    Name = "Desk A2",
                    IsActive = true,
                    DisplayOrder = 2,
                    PositionX = 2,
                    PositionY = 1
                },
                new Workstation
                {
                    Id = "ws-3",
                    OfficeId = office1.Id,
                    Code = "A3",
                    Name = "Desk A3",
                    IsActive = true,
                    DisplayOrder = 3,
                    PositionX = 3,
                    PositionY = 1
                },
                new Workstation
                {
                    Id = "ws-4",
                    OfficeId = office2.Id,
                    Code = "Q1",
                    Name = "Quiet Desk 1",
                    IsActive = true,
                    DisplayOrder = 1,
                    PositionX = 1,
                    PositionY = 1
                }
            };

            // ===== USERS =====
            var user1 = new AppUser
            {
                Id = "user-1",
                EntraObjectId = "entra-1",
                TenantId = "tenant-1",
                DisplayName = "András Bátori",
                Email = "batori@email.com",
                IsActive = true,
                CreatedAt = new DateTime(2026, 3, 27, 8, 30, 0, DateTimeKind.Utc)
            };

            var user2 = new AppUser
            {
                Id = "user-2",
                EntraObjectId = "entra-2",
                TenantId = "tenant-1",
                DisplayName = "Fenyvesi Péter",
                Email = "fenyvesi@email.com",
                IsActive = true,
                CreatedAt = new DateTime(2026, 3, 27, 8, 30, 0, DateTimeKind.Utc)
            };

            // ===== BOOKINGS =====
            var booking1 = new OfficeBooking
            {
                Id = "booking-1",
                WorkstationId = "ws-1",
                UserId = user1.Id,
                BookingDate = new DateOnly(2026, 3, 30),
                CreatedAtUtc = new DateTime(2026, 3, 27, 8, 30, 0, DateTimeKind.Utc),
                CreatedByUserId = user1.Id,
                IsCancelled = false
            };

            var booking2 = new OfficeBooking
            {
                Id = "booking-2",
                WorkstationId = "ws-2",
                UserId = user2.Id,
                BookingDate = new DateOnly(2026, 3, 30),
                CreatedAtUtc = new DateTime(2026, 3, 27, 8, 30, 0, DateTimeKind.Utc),
                CreatedByUserId = user2.Id,
                IsCancelled = false
            };

            modelBuilder.Entity<Location>().HasData(location1);
            modelBuilder.Entity<Office>().HasData(office1, office2);
            modelBuilder.Entity<Workstation>().HasData(workstations);
            modelBuilder.Entity<AppUser>().HasData(user1, user2);
            modelBuilder.Entity<OfficeBooking>().HasData(booking1, booking2);
        }
    }
}
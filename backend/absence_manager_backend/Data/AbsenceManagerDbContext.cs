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
        public DbSet<AbsenceRequest> AbsenceRequests => Set<AbsenceRequest>();

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
            // AbsenceRequest
            // -------------------------
            modelBuilder.Entity<AbsenceRequest>(entity =>
            {
                entity.ToTable("AbsenceRequests");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.UserId)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Type)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.DateFrom)
                    .IsRequired();

                entity.Property(x => x.DateTo)
                    .IsRequired();

                entity.Property(x => x.Reason)
                    .HasMaxLength(1000);

                entity.Property(x => x.CreatedAtUtc)
                    .IsRequired();

                entity.Property(x => x.CreatedByUserId)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.ReviewedByUserId)
                    .HasMaxLength(50);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.AbsenceRequests)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ReviewedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.ReviewedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.UserId, x.DateFrom, x.DateTo, x.Status })
                    .HasDatabaseName("IX_AbsenceRequests_User_DateRange_Status");
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

            

            var location1 = new Location
            {
                Id = "loc-Fót",
                Name = "Fót",
                IsActive = true,
                DisplayOrder = 1
            };

            // ===== OFFICE =====
            var office1 = new Office
            {
                Id = "office-ft-1",
                LocationId = location1.Id,
                Name = "113 - IT Office",
                Description = "IT fejlesztés",
                IsActive = true,
                DisplayOrder = 1
            };

            // ===== WORKSTATIONS =====
            var workstations = new List<Workstation>
            {
                new Workstation
                {
                    Id = "ws-1",
                    OfficeId = office1.Id,
                    Code = "KL",
                    Name = "1",
                    IsActive = true,
                    DisplayOrder = 1,
                    PositionX = 1,
                    PositionY = 1
                },
                new Workstation
                {
                    Id = "ws-2",
                    OfficeId = office1.Id,
                    Code = "GV",
                    Name = "2",
                    IsActive = true,
                    DisplayOrder = 2,
                    PositionX = 2,
                    PositionY = 1
                },
                new Workstation
                {
                    Id = "ws-3",
                    OfficeId = office1.Id,
                    Code = "KI",
                    Name = "3",
                    IsActive = true,
                    DisplayOrder = 3,
                    PositionX = 3,
                    PositionY = 1
                },
                new Workstation{
                    Id = "ws-4",
                    OfficeId = office1.Id,
                    Code = "PB",
                    Name = "4",
                    IsActive = true,
                    DisplayOrder = 4,
                    PositionX = 1,
                    PositionY = 2
                },
                new Workstation
                {
                    Id = "ws-5",
                    OfficeId = office1.Id,
                    Code = "Senki",
                    Name = "5",
                    IsActive = true,
                    DisplayOrder = 5,
                    PositionX = 2,
                    PositionY = 2
                },
                new Workstation
                {
                    Id = "ws-6",
                    OfficeId = office1.Id,
                    Code = "Szp",
                    Name = "6",
                    IsActive = true,
                    DisplayOrder = 6,
                    PositionX = 3,
                    PositionY = 2
                },
                new Workstation
                {
                    Id = "ws-7",
                    OfficeId = office1.Id,
                    Code = "Senki",
                    Name = "7",
                    IsActive = true,
                    DisplayOrder = 7,
                    PositionX = 2,
                    PositionY = 3
                },

            };


            modelBuilder.Entity<Location>().HasData(location1);
            modelBuilder.Entity<Office>().HasData(office1);
            modelBuilder.Entity<Workstation>().HasData(workstations);
            
        }
    }
}
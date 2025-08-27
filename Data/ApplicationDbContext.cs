using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EventEase.Models;
using System;
using System.Linq;

namespace EventEase.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Venue> Venues => Set<Venue>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Booking> Bookings => Set<Booking>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ---- VENUE ----
            b.Entity<Venue>(e =>
            {
                e.ToTable("Venues");

                e.HasKey(x => x.VenueId);

                e.Property(x => x.VenueName).IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
                e.Property(x => x.Location).IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
                e.Property(x => x.Capacity).IsRequired().HasColumnType("int");
                e.Property(x => x.ImageUrl).HasMaxLength(500).HasColumnType("nvarchar(500)");
                e.Property(x => x.IsActive).HasColumnType("bit").HasDefaultValue(true);
                e.Property(x => x.CreatedAt).IsRequired().HasColumnType("datetime2");
                e.Property(x => x.UpdatedAt).HasColumnType("datetime2");

                // ✅ Correct check constraint for Venue
                e.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_Venue_Capacity_Positive", "[Capacity] > 0");
                });

                e.HasIndex(x => x.VenueName);
                e.HasIndex(x => new { x.Location, x.IsActive });
            });


            // ---- EVENT ----
            b.Entity<Event>(e =>
            {
                e.ToTable("Events");

                e.HasKey(x => x.EventId);

                e.Property(x => x.EventName).IsRequired().HasMaxLength(200).HasColumnType("nvarchar(200)");
                e.Property(x => x.Description).HasColumnType("nvarchar(max)");
                e.Property(x => x.StartDateTime).HasColumnType("datetime2");
                e.Property(x => x.EndDateTime).HasColumnType("datetime2");
                e.Property(x => x.IsActive).HasColumnType("bit").HasDefaultValue(true);
                e.Property(x => x.CreatedAt).IsRequired().HasColumnType("datetime2");
                e.Property(x => x.UpdatedAt).HasColumnType("datetime2");
                e.Property(x => x.VenueId).IsRequired().HasColumnType("int");

                e.HasOne(x => x.Venue)
                 .WithMany()
                 .HasForeignKey(x => x.VenueId)
                 .OnDelete(DeleteBehavior.Restrict);

                // ✅ Correct check constraint for Event
                e.ToTable(tb =>
                {
                    tb.HasCheckConstraint(
                        "CK_Event_Time_Order",
                        "[StartDateTime] IS NULL OR [EndDateTime] IS NULL OR [StartDateTime] <= [EndDateTime]"
                    );
                });

                e.HasIndex(x => new { x.VenueId, x.StartDateTime });
                e.HasIndex(x => new { x.IsActive, x.EventName });
            });


            // ---- CUSTOMER ----
            b.Entity<Customer>(e =>
            {
                e.ToTable("Customers");

                e.HasKey(x => x.CustomerId);

                e.Property(x => x.FullName)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                e.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("nvarchar(200)");

                e.Property(x => x.Phone)
                    .HasMaxLength(50)
                    .HasColumnType("nvarchar(50)");

                e.Property(x => x.CreatedAt)
                    .IsRequired()
                    .HasColumnType("datetime2");

                e.Property(x => x.UpdatedAt)
                    .HasColumnType("datetime2");

                // Enforce unique emails if your business rules require it
                e.HasIndex(x => x.Email).IsUnique();

                e.HasIndex(x => x.FullName);
            });

            // ---- BOOKING ----
            b.Entity<Booking>(e =>
            {
                e.ToTable("Bookings");

                e.HasKey(x => x.BookingId);

                e.Property(x => x.BookingDate)
                    .IsRequired()
                    .HasColumnType("datetime2");

                e.Property(x => x.EventId)
                    .IsRequired()
                    .HasColumnType("int");

                e.Property(x => x.VenueId)
                    .IsRequired()
                    .HasColumnType("int");

                e.Property(x => x.CustomerId)
                    .IsRequired()
                    .HasColumnType("int");

                e.Property(x => x.CreatedByUserId)
                    .HasColumnType("nvarchar(450)");

                e.Property(x => x.UpdatedByUserId)
                    .HasColumnType("nvarchar(450)");

                e.Property(x => x.CreatedAt)
                    .IsRequired()
                    .HasColumnType("datetime2");

                e.Property(x => x.UpdatedAt)
                    .HasColumnType("datetime2");

                e.HasOne(x => x.Event)
                    .WithMany()
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Venue)
                    .WithMany()
                    .HasForeignKey(x => x.VenueId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.Customer)
                    .WithMany()
                    .HasForeignKey(x => x.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Auditing to AspNetUsers (no cascade to avoid multiple cascade paths)
                e.HasOne<IdentityUser>()
                 .WithMany()
                 .HasForeignKey(x => x.CreatedByUserId)
                 .OnDelete(DeleteBehavior.NoAction);

                e.HasOne<IdentityUser>()
                 .WithMany()
                 .HasForeignKey(x => x.UpdatedByUserId)
                 .OnDelete(DeleteBehavior.NoAction);


                // Prevent duplicate bookings by the same customer for the same event (optional)
                e.HasIndex(x => new { x.EventId, x.CustomerId })
                    .IsUnique()
                    .HasDatabaseName("UQ_Booking_Event_Customer");

                // Common query patterns
                e.HasIndex(x => new { x.BookingDate, x.EventId });
            });

            // ----------------- OPTIONAL SEED DATA -----------------
            // (Safe example seeds—adjust or remove for production)
            /*
            b.Entity<Venue>().HasData(
                new Venue { VenueId = 1, VenueName = "Main Hall", Location = "Campus A", Capacity = 300, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Venue { VenueId = 2, VenueName = "Auditorium", Location = "Campus B", Capacity = 500, IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            b.Entity<Event>().HasData(
                new Event { EventId = 1, EventName = "Tech Talk", VenueId = 1, StartDateTime = DateTime.UtcNow.AddDays(7), EndDateTime = DateTime.UtcNow.AddDays(7).AddHours(2), IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            b.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, FullName = "Alex Smith", Email = "alex@example.com", CreatedAt = DateTime.UtcNow }
            );

            b.Entity<Booking>().HasData(
                new Booking { BookingId = 1, EventId = 1, VenueId = 1, CustomerId = 1, BookingDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow }
            );
            */
        }

        // Auto timestamps for all tracked entities
        public override int SaveChanges()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries()
                         .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                // Apply to entities with these properties
                if (entry.Metadata.FindProperty("CreatedAt") != null)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.CurrentValues["CreatedAt"] = now;
                        if (entry.Metadata.FindProperty("UpdatedAt") != null)
                            entry.CurrentValues["UpdatedAt"] = null;
                    }
                    else if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("UpdatedAt") != null)
                    {
                        entry.CurrentValues["UpdatedAt"] = now;
                    }
                }
            }

            return base.SaveChanges();
        }
    }
}

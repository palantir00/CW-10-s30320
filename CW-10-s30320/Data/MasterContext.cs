using Microsoft.EntityFrameworkCore;
using CW_10_s30320.Models;   // <- dokładnie ta sama przestrzeń nazw, w jakiej znajdują się Twoje modele

namespace CW_10_s30320.Data
{
    public class MasterContext : DbContext
    {
        public MasterContext(DbContextOptions<MasterContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Client_Trip> Client_Trips { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Country_Trip> Country_Trips { get; set; }
        public DbSet<Trip> Trips { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.IdClient);
                entity.Property(e => e.FirstName).HasMaxLength(120).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(120).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(120).IsRequired();
                entity.Property(e => e.Telephone).HasMaxLength(30).IsRequired();
                entity.Property(e => e.Pesel).HasMaxLength(11).IsRequired();

                entity.ToTable("Client");
            });
            
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.IdCountry);
                entity.Property(e => e.Name).HasMaxLength(120).IsRequired();

                entity.ToTable("Country");
            });

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.HasKey(e => e.IdTrip);
                entity.Property(e => e.Name).HasMaxLength(120).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(220).IsRequired();
                entity.Property(e => e.DateFrom).HasColumnType("date").IsRequired();
                entity.Property(e => e.DateTo).HasColumnType("date").IsRequired();
                entity.Property(e => e.MaxPeople).IsRequired();

                entity.ToTable("Trip");
            });

            modelBuilder.Entity<Client_Trip>(entity =>
            {
                entity.HasKey(ct => new { ct.IdClient, ct.IdTrip });

                entity.Property(ct => ct.RegistrationDate).HasColumnType("date").IsRequired();
                entity.Property(ct => ct.PaymentDate).HasColumnType("date").IsRequired(false);

                entity.HasOne(ct => ct.IdClientNavigation)
                      .WithMany(c => c.Client_Trips)
                      .HasForeignKey(ct => ct.IdClient);

                entity.HasOne(ct => ct.IdTripNavigation)
                      .WithMany(t => t.Client_Trips)
                      .HasForeignKey(ct => ct.IdTrip);

                entity.ToTable("Client_Trip");
            });
            
            modelBuilder.Entity<Country_Trip>(entity =>
            {
                entity.HasKey(ct => new { ct.IdCountry, ct.IdTrip });

                entity.HasOne(ct => ct.IdCountryNavigation)
                      .WithMany(c => c.Country_Trips)
                      .HasForeignKey(ct => ct.IdCountry);

                entity.HasOne(ct => ct.IdTripNavigation)
                      .WithMany(t => t.Country_Trips)
                      .HasForeignKey(ct => ct.IdTrip);

                entity.ToTable("Country_Trip");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

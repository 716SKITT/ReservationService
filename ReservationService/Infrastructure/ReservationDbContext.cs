using ReservationService.Models;
using Microsoft.EntityFrameworkCore;

namespace ReservationService.Infrastructure;

public class ReservationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    public ReservationDbContext(DbContextOptions<ReservationDbContext> options)
            : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Reservation>()
            .HasKey(r => r.Id);
    }
}
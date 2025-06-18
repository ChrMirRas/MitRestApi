using Microsoft.EntityFrameworkCore;
using MitRestApi.Models; // Tilføj denne linje, hvis dine modeller ligger i en Models-mappe

using MitRestApi.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();  // 👈 Dette er det vigtige
    }
}

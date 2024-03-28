using CatTok.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatTok.Infrastructure;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasKey(u => u.Id);
            user.Property(u => u.Sub).IsRequired(false);
            user.Property(u => u.IsGoogleUser).HasDefaultValue(false);
            user.Property(u => u.Picture).IsRequired(false);
            user.Property(u => u.PasswordHash).IsRequired(false);
        });
    }
}
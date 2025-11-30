using eCommerce.UserService.Domain.AggregatesModel.UserAggregate;
using eCommerce.UserService.Domain.References;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.UserService.Infrastructure;

/// <summary>
/// Entity Framework Core DbContext for the eCommerce domain.
/// </summary>
public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefOrder> RefOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.CreatedAt)
                .IsRequired();

            entity.HasIndex(u => u.Email)
                .IsUnique();
        });

        // Configure RefOrder entity
        modelBuilder.Entity<RefOrder>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.UserId)
                .IsRequired();

            entity.Property(o => o.Product)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(o => o.Quantity)
                .IsRequired();

            entity.Property(o => o.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(o => o.CreatedAt)
                .IsRequired();

            entity.Property(o => o.UpdatedAt);

            entity.HasIndex(o => o.UserId);
        });
    }
}

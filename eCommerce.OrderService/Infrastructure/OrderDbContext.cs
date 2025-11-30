using Microsoft.EntityFrameworkCore;
using eCommerce.OrderService.Domain.AggregatesModel.OrderAggregate;
using eCommerce.OrderService.Domain.References;

namespace eCommerce.OrderService.Infrastructure;

/// <summary>
/// Entity Framework Core DbContext for the eCommerce Order domain.
/// </summary>
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<RefUser> RefUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
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

            entity.HasIndex(o => o.UserId);
        });

        // Configure RefUser entity
        modelBuilder.Entity<RefUser>(entity =>
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

            entity.Property(u => u.UpdatedAt);

            entity.HasIndex(u => u.Email)
                .IsUnique();
        });
    }
}

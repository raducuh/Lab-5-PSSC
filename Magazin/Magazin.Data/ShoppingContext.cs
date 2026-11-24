using Magazin.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Magazin.Data;

/// <summary>Contextul EF Core pentru schema descrisă în <c>SQL/create-magazin-db.sql</c>.</summary>
public sealed class ShoppingContext(DbContextOptions<ShoppingContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();

    public DbSet<OrderEntity> Orders => Set<OrderEntity>();

    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(product =>
        {
            product.ToTable("Products");
            product.HasKey(p => p.Id);
            product.Property(p => p.Code).HasMaxLength(20).IsUnicode(false);
            product.HasIndex(p => p.Code).IsUnique().HasDatabaseName("UQ_Products_Code");
            product.Property(p => p.Name).HasMaxLength(100);

            // decimal(10,2), la fel ca în SQL/create-magazin-db.sql.
            product.Property(p => p.Price).HasPrecision(10, 2);
            product.Property(p => p.QuantityType).HasMaxLength(10);

            // Stocul este și jeton de concurență: dacă între citire și salvare altcineva l-a modificat, UPDATE-ul
            // (care include "AND Stock = valoarea citită") nu afectează niciun rând, iar EF Core aruncă
            // DbUpdateConcurrencyException în loc să scadă stocul de două ori. CHECK (Stock >= 0) rămâne doar în SQL.
            product.Property(p => p.Stock).IsConcurrencyToken();
        });

        modelBuilder.Entity<CustomerEntity>(customer =>
        {
            customer.ToTable("Customers");
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Code).HasMaxLength(20).IsUnicode(false);
            customer.HasIndex(c => c.Code).IsUnique().HasDatabaseName("UQ_Customers_Code");
            customer.Property(c => c.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<OrderEntity>(order =>
        {
            // "Order" este cuvânt rezervat în SQL; EF Core îl pune singur între paranteze drepte în comenzile generate.
            order.ToTable("Order");
            order.HasKey(o => o.Id);
            order.Property(o => o.DeliveryAddress).HasMaxLength(200);

            order.HasOne(o => o.Customer)
                 .WithMany(c => c.Orders)
                 .HasForeignKey(o => o.CustomerId)
                 .HasConstraintName("FK_Order_Customers");
        });

        modelBuilder.Entity<OrderItemEntity>(item =>
        {
            item.ToTable("OrderItem");
            item.HasKey(i => i.Id);

            item.HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .HasConstraintName("FK_OrderItem_Order");

            item.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .HasConstraintName("FK_OrderItem_Products");
        });
    }
}

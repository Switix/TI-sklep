using Microsoft.EntityFrameworkCore;
using TISklep.Service.Model;

namespace TISklep.Service.Application;

public class BoardGamesContext(DbContextOptions<BoardGamesContext> options) : DbContext(options)
{
    public DbSet<BoardGame> BoardGames { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<BoardGame>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.Stock)
                .IsRequired();

            entity.HasOne(e => e.Category)
                .WithMany(c => c.BoardGames)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.KeycloakId)
                .IsUnique();

            entity.Property(e => e.KeycloakId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .HasMaxLength(200);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TotalPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.BoardGame)
                .WithMany()
                .HasForeignKey(e => e.BoardGameId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Strategiczne", Description = "Gry wymagające planowania i taktyki." },
            new Category { Id = 2, Name = "Familijne", Description = "Idealne do gry z rodziną i dziećmi." },
            new Category { Id = 3, Name = "Imprezowe", Description = "Szybkie, proste zasady, dużo śmiechu." }
        );

        modelBuilder.Entity<BoardGame>().HasData(
            new BoardGame
            {
                Id = 1, CategoryId = 1, Title = "Terraformacja Marsa", Price = 199.99m, Stock = 10,
                Description = "Zmień Czerwoną Planetę w raj."
            },
            new BoardGame
            {
                Id = 2, CategoryId = 1, Title = "Scythe", Price = 319.00m, Stock = 5,
                Description = "Zarządzaj frakcją w alternatywnej Europie lat 20."
            },
            new BoardGame
            {
                Id = 3, CategoryId = 2, Title = "Wsiąść do Pociągu: Europa", Price = 169.50m, Stock = 15,
                Description = "Buduj trasy kolejowe przez całą Europę."
            },
            new BoardGame
            {
                Id = 4, CategoryId = 2, Title = "Catan", Price = 149.00m, Stock = 12,
                Description = "Handluj i buduj osady na wyspie Catan."
            },
            new BoardGame
            {
                Id = 5, CategoryId = 3, Title = "Tajniacy", Price = 69.99m, Stock = 20,
                Description = "Odgadnij tożsamość swoich agentów na podstawie haseł."
            }
        );
    }
}
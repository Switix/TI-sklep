using Microsoft.EntityFrameworkCore;
using TISklep.Service.Model;

namespace TISklep.Service.Application;

public class BoardGamesContext(DbContextOptions<BoardGamesContext> options) : DbContext(options)
{
    public DbSet<BoardGame> BoardGames { get; set; }
    public DbSet<Category> Categories { get; set; }

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

            entity.HasOne(e => e.Category)
                .WithMany(c => c.BoardGames)
                .HasForeignKey(e => e.CategoryId)
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
                Id = 1, CategoryId = 1, Title = "Terraformacja Marsa", Price = 199.99m,
                Description = "Zmień Czerwoną Planetę w raj."
            },
            new BoardGame
            {
                Id = 2, CategoryId = 1, Title = "Scythe", Price = 319.00m,
                Description = "Zarządzaj frakcją w alternatywnej Europie lat 20."
            },
            new BoardGame
            {
                Id = 3, CategoryId = 2, Title = "Wsiąść do Pociągu: Europa", Price = 169.50m,
                Description = "Buduj trasy kolejowe przez całą Europę."
            },
            new BoardGame
            {
                Id = 4, CategoryId = 2, Title = "Catan", Price = 149.00m,
                Description = "Handluj i buduj osady na wyspie Catan."
            },
            new BoardGame
            {
                Id = 5, CategoryId = 3, Title = "Tajniacy", Price = 69.99m,
                Description = "Odgadnij tożsamość swoich agentów na podstawie haseł."
            }
        );
    }
}
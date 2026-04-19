using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MealMind.Models;

public partial class MealMindDbContext : DbContext
{
    public MealMindDbContext()
    {
    }

    public MealMindDbContext(DbContextOptions<MealMindDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categorie> Categories { get; set; }
    public virtual DbSet<Gebruiker> Gebruikers { get; set; }
    public virtual DbSet<Gebruikerkiestrecept> Gebruikerkiestrecepts { get; set; }
    public virtual DbSet<Ingredient> Ingredients { get; set; }
    public virtual DbSet<Recept> Recepts { get; set; }
    public virtual DbSet<ReceptHasIngredient> ReceptHasIngredients { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, move it out of source code.
        => optionsBuilder.UseMySql("server=localhost;database=MealMind;user=root;password=1234", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.4.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb3_general_ci")
            .HasCharSet("utf8mb3");

        modelBuilder.Entity<Categorie>(entity =>
        {
            entity.HasKey(e => e.IdCategorie).HasName("PRIMARY");
            entity.ToTable("categorie");
            entity.Property(e => e.IdCategorie).HasColumnName("idCategorie");
            entity.Property(e => e.CategorieNaam).HasColumnType("mediumtext");
        });

        modelBuilder.Entity<Gebruiker>(entity =>
        {
            entity.HasKey(e => e.IdGebruiker).HasName("PRIMARY");
            entity.ToTable("gebruiker");
            entity.Property(e => e.IdGebruiker).HasColumnName("idGebruiker");

            // Map only existing columns
            entity.Property(e => e.GebruikerUsername).HasColumnName("GebruikerUsername").HasColumnType("mediumtext");
            entity.Property(e => e.GebruikerEmail).HasColumnName("GebruikerEmail").HasColumnType("mediumtext");
            entity.Property(e => e.GebruikerPasswordHash).HasColumnName("GebruikerPasswordHash").HasColumnType("mediumtext");
        });

        modelBuilder.Entity<Gebruikerkiestrecept>(entity =>
        {
            entity.HasKey(e => new { e.FkGebruiker, e.Datum })
    .HasName("PRIMARY");

            entity.ToTable("gebruikerkiestrecept");

            entity.HasIndex(e => e.FkGebruiker, "fk_Recept_has_Gebruiker_Gebruiker1_idx");
            entity.HasIndex(e => e.FkRecept, "fk_Recept_has_Gebruiker_Recept1_idx");

            entity.HasOne(d => d.FkGebruikerNavigation).WithMany(p => p.Gebruikerkiestrecepts)
                .HasForeignKey(d => d.FkGebruiker)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Recept_has_Gebruiker_Gebruiker1");

            entity.HasOne(d => d.FkReceptNavigation).WithMany(p => p.Gebruikerkiestrecepts)
                .HasForeignKey(d => d.FkRecept)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Recept_has_Gebruiker_Recept1");
        });

        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.IdIngredient).HasName("PRIMARY");
            entity.ToTable("ingredient");
            entity.Property(e => e.IdIngredient).HasColumnName("idIngredient");
            entity.Property(e => e.IngredientNaam).HasColumnType("mediumtext");
        });

        modelBuilder.Entity<Recept>(entity =>
        {
            entity.HasKey(e => e.IdRecept).HasName("PRIMARY");
            entity.ToTable("recept");
            entity.Property(e => e.IdRecept).HasColumnName("idRecept");
            entity.Property(e => e.ReceptBeshrijving).HasMaxLength(45);
            entity.Property(e => e.ReceptNaam).HasColumnType("mediumtext");

            entity.HasMany(r => r.FkCategories)
                .WithMany(c => c.FkRecepts)
                .UsingEntity<Dictionary<string, object>>(
                    "categorie_has_recept",
                    j => j.HasOne<Categorie>()
                          .WithMany()
                          .HasForeignKey("FkCategorie")
                          .HasConstraintName("fk_categorie_has_recept_categorie"),
                    j => j.HasOne<Recept>()
                          .WithMany()
                          .HasForeignKey("FkRecept")
                          .HasConstraintName("fk_categorie_has_recept_recept"),
                    j =>
                    {
                        j.HasKey("FkRecept", "FkCategorie");
                        j.ToTable("categorie_has_recept");
                    });
        });

        modelBuilder.Entity<ReceptHasIngredient>(entity =>
        {
            entity.HasKey(e => new { e.FkRecept, e.FkIngredient })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("recept_has_ingredient");

            entity.HasIndex(e => e.FkIngredient, "fk_Recept_has_Ingredient_Ingredient1_idx");
            entity.HasIndex(e => e.FkRecept, "fk_Recept_has_Ingredient_Recept1_idx");

            entity.Property(e => e.HoeveelheidIngredient).HasColumnType("mediumtext");

            entity.HasOne(d => d.FkIngredientNavigation).WithMany(p => p.ReceptHasIngredients)
                .HasForeignKey(d => d.FkIngredient)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Recept_has_Ingredient_Ingredient1");

            entity.HasOne(d => d.FkReceptNavigation).WithMany(p => p.ReceptHasIngredients)
                .HasForeignKey(d => d.FkRecept)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Recept_has_Ingredient_Recept1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

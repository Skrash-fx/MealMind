using Microsoft.EntityFrameworkCore;

namespace MealMindAPI.Models;

public class MealMindDbContext : DbContext
{
    public MealMindDbContext(DbContextOptions<MealMindDbContext> options) : base(options) { }

    public DbSet<Categorie> Categories { get; set; }
    public DbSet<Recept> Recepten { get; set; }
    public DbSet<Ingredient> Ingredienten { get; set; }
    public DbSet<ReceptHasIngredient> ReceptHasIngredienten { get; set; }
    public DbSet<Gebruiker> Gebruikers { get; set; }
    public DbSet<GebruikerKiestRecept> GebruikerKiestRecepten { get; set; }
    public DbSet<CategorieHasRecept> CategorieHasRecepten { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReceptHasIngredient>()
            .HasKey(r => new { r.FkRecept, r.FkIngredient });

        modelBuilder.Entity<GebruikerKiestRecept>()
            .HasKey(g => new { g.FkRecept, g.FkGebruiker });

        modelBuilder.Entity<CategorieHasRecept>()
            .HasKey(c => new { c.FkCategorie, c.FkRecept });
    }
}

public class Categorie
{
    public int IdCategorie { get; set; }
    public string CategorieNaam { get; set; } = string.Empty;
    public ICollection<CategorieHasRecept> CategorieHasRecepten { get; set; } = new List<CategorieHasRecept>();
}

public class Recept
{
    public int IdRecept { get; set; }
    public string ReceptNaam { get; set; } = string.Empty;
    public string ReceptBeshrijving { get; set; } = string.Empty;
    public ICollection<ReceptHasIngredient> ReceptHasIngredienten { get; set; } = new List<ReceptHasIngredient>();
    public ICollection<CategorieHasRecept> CategorieHasRecepten { get; set; } = new List<CategorieHasRecept>();
    public ICollection<GebruikerKiestRecept> GebruikerKiestRecepten { get; set; } = new List<GebruikerKiestRecept>();
}

public class Ingredient
{
    public int IdIngredient { get; set; }
    public string IngredientNaam { get; set; } = string.Empty;
    public ICollection<ReceptHasIngredient> ReceptHasIngredienten { get; set; } = new List<ReceptHasIngredient>();
}

public class ReceptHasIngredient
{
    public int FkRecept { get; set; }
    public int FkIngredient { get; set; }
    public string HoeveelheidIngredient { get; set; } = string.Empty;
    public Recept FkReceptNavigation { get; set; } = null!;
    public Ingredient FkIngredientNavigation { get; set; } = null!;
}

public class Gebruiker
{
    public int IdGebruiker { get; set; }
    public string GebruikerVoorNaam { get; set; } = string.Empty;
    public string GebruikerAchternaam { get; set; } = string.Empty;
    public string GebruikerGsm { get; set; } = string.Empty;
    public ICollection<GebruikerKiestRecept> GebruikerKiestRecepten { get; set; } = new List<GebruikerKiestRecept>();
}

public class GebruikerKiestRecept
{
    public int FkRecept { get; set; }
    public int FkGebruiker { get; set; }
    public DateOnly? Datum { get; set; }
    public Recept FkReceptNavigation { get; set; } = null!;
    public Gebruiker FkGebruikerNavigation { get; set; } = null!;
}

public class CategorieHasRecept
{
    public int FkCategorie { get; set; }
    public int FkRecept { get; set; }
    public Categorie FkCategorieNavigation { get; set; } = null!;
    public Recept FkReceptNavigation { get; set; } = null!;
}

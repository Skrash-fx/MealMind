using MealMindAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Server=localhost;Database=mealmind;User=root;Password=1234;";

builder.Services.AddDbContext<MealMindDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// ──────────────────────────────────────────────
// GEBRUIKER endpoints
// ──────────────────────────────────────────────

// GET: Login
app.MapGet("/login", async (string gsm, MealMindDbContext db) =>
{
    var user = await db.Gebruikers
        .Where(g => g.GebruikerGsm == gsm)
        .Select(g => new { g.IdGebruiker, g.GebruikerVoorNaam, g.GebruikerAchternaam })
        .FirstOrDefaultAsync();

    if (user == null)
        return Results.Unauthorized();

    return Results.Ok(user);
});

// POST: Registreer gebruiker
app.MapPost("/gebruiker", async (string voornaam, string achternaam, string gsm, MealMindDbContext db) =>
{
    var exists = await db.Gebruikers.AnyAsync(g => g.GebruikerGsm == gsm);
    if (exists)
        return Results.Conflict("Gebruiker met dit GSM-nummer bestaat al.");

    var gebruiker = new Gebruiker
    {
        GebruikerVoorNaam = voornaam,
        GebruikerAchternaam = achternaam,
        GebruikerGsm = gsm
    };

    db.Gebruikers.Add(gebruiker);
    await db.SaveChangesAsync();
    return Results.Created("/gebruiker", gebruiker);
});

// ──────────────────────────────────────────────
// RECEPT endpoints
// ──────────────────────────────────────────────

// GET: Alle recepten (met categorieën)
app.MapGet("/recepten", async (MealMindDbContext db) =>
{
    var recepten = await db.Recepten
        .Select(r => new
        {
            r.IdRecept,
            r.ReceptNaam,
            r.ReceptBeshrijving,
            Categorieen = r.CategorieHasRecepten
                .Select(c => c.FkCategorieNavigation.CategorieNaam)
                .ToList()
        })
        .ToListAsync();

    return Results.Ok(recepten);
});

// GET: Één recept met ingrediënten
app.MapGet("/recepten/{id}", async (int id, MealMindDbContext db) =>
{
    var recept = await db.Recepten
        .Where(r => r.IdRecept == id)
        .Select(r => new
        {
            r.IdRecept,
            r.ReceptNaam,
            r.ReceptBeshrijving,
            Categorieen = r.CategorieHasRecepten
                .Select(c => c.FkCategorieNavigation.CategorieNaam)
                .ToList(),
            Ingredienten = r.ReceptHasIngredienten
                .Select(i => new
                {
                    i.FkIngredientNavigation.IngredientNaam,
                    i.HoeveelheidIngredient
                })
                .ToList()
        })
        .FirstOrDefaultAsync();

    if (recept == null) return Results.NotFound();
    return Results.Ok(recept);
});

// POST: Voeg recept toe
app.MapPost("/recepten", async (string naam, string beschrijving, MealMindDbContext db) =>
{
    var recept = new Recept { ReceptNaam = naam, ReceptBeshrijving = beschrijving };
    db.Recepten.Add(recept);
    await db.SaveChangesAsync();
    return Results.Created($"/recepten/{recept.IdRecept}", recept);
});

// DELETE: Verwijder recept
app.MapDelete("/recepten/{id}", async (int id, MealMindDbContext db) =>
{
    var recept = await db.Recepten.FindAsync(id);
    if (recept == null) return Results.NotFound();
    db.Recepten.Remove(recept);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ──────────────────────────────────────────────
// GEBRUIKER KIEST RECEPT endpoints
// ──────────────────────────────────────────────

// GET: Geplande recepten van een gebruiker
app.MapGet("/gebruikers/{userId}/planning", async (int userId, MealMindDbContext db) =>
{
    var planning = await db.GebruikerKiestRecepten
        .Where(g => g.FkGebruiker == userId)
        .Select(g => new
        {
            g.FkRecept,
            ReceptNaam = g.FkReceptNavigation.ReceptNaam,
            ReceptBeschrijving = g.FkReceptNavigation.ReceptBeshrijving,
            g.Datum
        })
        .OrderBy(g => g.Datum)
        .ToListAsync();

    return Results.Ok(planning);
});

// POST: Plan een recept voor een gebruiker
app.MapPost("/gebruikers/{userId}/planning/{receptId}", async (int userId, int receptId, DateOnly datum, MealMindDbContext db) =>
{
    var exists = await db.GebruikerKiestRecepten
        .AnyAsync(g => g.FkGebruiker == userId && g.FkRecept == receptId);

    if (exists)
        return Results.Conflict("Recept staat al in de planning.");

    var item = new GebruikerKiestRecept
    {
        FkGebruiker = userId,
        FkRecept = receptId,
        Datum = datum
    };

    db.GebruikerKiestRecepten.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/gebruikers/{userId}/planning", item);
});

// DELETE: Verwijder recept uit planning
app.MapDelete("/gebruikers/{userId}/planning/{receptId}", async (int userId, int receptId, MealMindDbContext db) =>
{
    var item = await db.GebruikerKiestRecepten
        .FirstOrDefaultAsync(g => g.FkGebruiker == userId && g.FkRecept == receptId);

    if (item == null) return Results.NotFound();
    db.GebruikerKiestRecepten.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ──────────────────────────────────────────────
// CATEGORIE endpoints
// ──────────────────────────────────────────────

// GET: Alle categorieën
app.MapGet("/categorieen", async (MealMindDbContext db) =>
{
    var cats = await db.Categories
        .Select(c => new { c.IdCategorie, c.CategorieNaam })
        .ToListAsync();
    return Results.Ok(cats);
});

app.Run();

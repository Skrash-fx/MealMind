using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MealMind.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<MealMindDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Registration endpoint: accepts Voornaam (display name), optional Username, Email and Password
app.MapPost("/gebruikers", async (RegistrationRequest req, MealMindDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Voornaam) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Voornaam, email en password zijn verplicht.");

    var email = req.Email.Trim().ToLowerInvariant();
    var username = string.IsNullOrWhiteSpace(req.Username) ? (email.Contains('@') ? email.Split('@')[0] : email) : req.Username.Trim();

    // uniqueness checks
    var emailExists = await db.Gebruikers.AnyAsync(g => g.GebruikerEmail != null && g.GebruikerEmail.ToLower() == email);
    if (emailExists) return Results.Conflict("Email already in use");

    var usernameExists = await db.Gebruikers.AnyAsync(g => g.GebruikerUsername != null && g.GebruikerUsername.ToLower() == username.ToLower());
    if (usernameExists) return Results.Conflict("Username already in use");

    var gebruiker = new Gebruiker
    {
        GebruikerUsername = username,
        GebruikerEmail = email
    };

    var hasher = new PasswordHasher<Gebruiker>();
    gebruiker.GebruikerPasswordHash = hasher.HashPassword(gebruiker, req.Password);

    db.Gebruikers.Add(gebruiker);
    await db.SaveChangesAsync();

    return Results.Created($"/gebruikers/{gebruiker.IdGebruiker}", new
    {
        gebruiker.IdGebruiker,
        gebruiker.GebruikerUsername,
        gebruiker.GebruikerEmail
    });
});

// Login endpoint: accepts identifier (username or email) + password
// If stored password looks like plaintext and matches, convert to hashed value (one-time) and save.
app.MapPost("/gebruikers/login", async (LoginRequest req, MealMindDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(req.Identifier) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Identifier and password required.");

    var identifier = req.Identifier.Trim();

    Gebruiker? gebruiker = null;
    if (identifier.Contains('@'))
    {
        var email = identifier.ToLowerInvariant();
        gebruiker = await db.Gebruikers.FirstOrDefaultAsync(g => g.GebruikerEmail != null && g.GebruikerEmail.ToLower() == email);
    }
    else
    {
        var uname = identifier;
        gebruiker = await db.Gebruikers.FirstOrDefaultAsync(g => g.GebruikerUsername != null && g.GebruikerUsername == uname);
    }

    if (gebruiker == null || string.IsNullOrEmpty(gebruiker.GebruikerPasswordHash))
        return Results.Unauthorized();

    var stored = gebruiker.GebruikerPasswordHash ?? string.Empty;

    bool IsProbablyHashed(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        if (s.StartsWith("AQAAAA")) return true; // typical ASP.NET Identity prefix
        if (s.StartsWith("$")) return true; // other bcrypt/argon formats
        return s.Length > 30; // heuristic
    }

    var hasher = new PasswordHasher<Gebruiker>();

    if (IsProbablyHashed(stored))
    {
        var verify = hasher.VerifyHashedPassword(gebruiker, stored, req.Password);
        if (verify == PasswordVerificationResult.Failed)
            return Results.Unauthorized();
    }
    else
    {
        // stored looks like plaintext — compare directly, then replace with hashed value if matches
        if (stored != req.Password)
            return Results.Unauthorized();

        // re-hash and persist
        gebruiker.GebruikerPasswordHash = hasher.HashPassword(gebruiker, req.Password);
        db.Gebruikers.Update(gebruiker);
        await db.SaveChangesAsync();
    }

    return Results.Ok(new
    {
        gebruiker.IdGebruiker,
        gebruiker.GebruikerUsername,
        gebruiker.GebruikerEmail
    });
});

// Categories
app.MapGet("/categorieen", async (MealMindDbContext db) =>
{
    var cats = await db.Categories.ToListAsync();
    return Results.Ok(cats);
});

// Recepten list (robust: haalt categorieën via join-table 'categorie_has_recept')
app.MapGet("/recepten", async (MealMindDbContext db) =>
{
    try
    {
        // Haal alle recepten (basisvelden)
        var receptenBase = await db.Recepts
            .Select(r => new { r.IdRecept, r.ReceptNaam, r.ReceptBeshrijving })
            .ToListAsync();

        var result = new List<object>();

        foreach (var r in receptenBase)
        {
            // Haal categorie-namen via expliciete join op de bestaande join-table
            var categorieen = await db.Categories
                .FromSqlRaw(
                    @"SELECT c.* 
                      FROM categorie c
                      JOIN categorie_has_recept j ON j.FkCategorie = c.idCategorie
                      WHERE j.FkRecept = {0}", r.IdRecept)
                .Select(c => c.CategorieNaam)
                .ToListAsync();

            result.Add(new
            {
                IdRecept = r.IdRecept,
                ReceptNaam = r.ReceptNaam,
                ReceptBeshrijving = r.ReceptBeshrijving,
                Categorieen = categorieen
            });
        }

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        // Return a friendly error and avoid crashing Swagger UI
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
});

// Recept detail
app.MapGet("/recepten/{id}", async (int id, MealMindDbContext db) =>
{
    var recept = await db.Recepts
        .Include(r => r.ReceptHasIngredients)
            .ThenInclude(ri => ri.FkIngredientNavigation)
        .Include(r => r.FkCategories)
        .Where(r => r.IdRecept == id)
        .Select(r => new
        {
            r.IdRecept,
            r.ReceptNaam,
            r.ReceptBeshrijving,
            Ingredienten = r.ReceptHasIngredients.Select(ri => new
            {
                ri.FkIngredientNavigation.IngredientNaam,
                ri.HoeveelheidIngredient
            }).ToList(),
            Categorieen = r.FkCategories.Select(c => c.CategorieNaam).ToList()
        })
        .FirstOrDefaultAsync();

    if (recept == null) return Results.NotFound();
    return Results.Ok(recept);
});

// Create recept
app.MapPost("/recepten", async (ReceptRequest req, MealMindDbContext db) =>
{
    var recept = new Recept
    {
        ReceptNaam = req.Naam,
        ReceptBeshrijving = req.Beschrijving
    };
    db.Recepts.Add(recept);
    await db.SaveChangesAsync();

    foreach (var ing in req.Ingredienten)
    {
        var ingredient = await db.Ingredients
            .FirstOrDefaultAsync(i => i.IngredientNaam == ing.Naam)
            ?? new Ingredient { IngredientNaam = ing.Naam };

        if (ingredient.IdIngredient == 0)
        {
            db.Ingredients.Add(ingredient);
            await db.SaveChangesAsync();
        }

        db.ReceptHasIngredients.Add(new ReceptHasIngredient
        {
            FkRecept = recept.IdRecept,
            FkIngredient = ingredient.IdIngredient,
            HoeveelheidIngredient = ing.Hoeveelheid
        });
    }

    foreach (var catId in req.CategorieIds)
    {
        var categorie = await db.Categories.FindAsync(catId);
        if (categorie != null)
            recept.FkCategories.Add(categorie);
    }

    await db.SaveChangesAsync();
    return Results.Created($"/recepten/{recept.IdRecept}", recept);
});

// User planning for 7 days
app.MapGet("/gebruikers/{id}/planning", async (int id, MealMindDbContext db) =>
{
    var planning = await db.Gebruikerkiestrecepts
        .Where(g => g.FkGebruiker == id)
        .Include(g => g.FkReceptNavigation)
        .Select(g => new
        {
            g.Datum,
            g.FkRecept,
            ReceptNaam = g.FkReceptNavigation.ReceptNaam,
            Beschrijving = g.FkReceptNavigation.ReceptBeshrijving
        })
        .OrderBy(g => g.Datum)
        .ToListAsync();

    return Results.Ok(planning);
});

// Generate planning
app.MapPost("/gebruikers/{id}/planning/genereer", async (int id, MealMindDbContext db) =>
{
    var bestaande = db.Gebruikerkiestrecepts.Where(g => g.FkGebruiker == id);
    db.Gebruikerkiestrecepts.RemoveRange(bestaande);

    var categorieen = await db.Categories.Include(c => c.FkRecepts).ToListAsync();
    var random = new Random();
    var vandaag = DateOnly.FromDateTime(DateTime.Today);

    for (int dag = 0; dag < 7; dag++)
    {
        var cat = categorieen[random.Next(categorieen.Count)];
        var receptenInCat = cat.FkRecepts.ToList();

        if (!receptenInCat.Any()) continue;

        var recept = receptenInCat[random.Next(receptenInCat.Count)];

        db.Gebruikerkiestrecepts.Add(new Gebruikerkiestrecept
        {
            FkGebruiker = id,
            FkRecept = recept.IdRecept,
            Datum = vandaag.AddDays(dag)
        });
    }

    await db.SaveChangesAsync();
    return Results.Ok("Planning tegenereerd");
});

// Update day's recipe
app.MapPut("/gebruikers/{id}/planning/{datum}", async (int id, string datum, PlanningUpdate update, MealMindDbContext db) =>
{
    var date = DateOnly.Parse(datum);

    var item = await db.Gebruikerkiestrecepts
        .FirstOrDefaultAsync(g => g.FkGebruiker == id && g.Datum == date);

    if (item == null)
    {
        db.Gebruikerkiestrecepts.Add(new Gebruikerkiestrecept
        {
            FkGebruiker = id,
            FkRecept = update.ReceptId,
            Datum = date
        });
    }
    else
    {
        item.FkRecept = update.ReceptId;
    }

    await db.SaveChangesAsync();
    return Results.Ok("Dag aangepast");
});

// DEV-ONLY: hash existing plaintext passwords for users and replace with PasswordHasher output.
// Add this before `app.Run();`. Remove this endpoint after you used it once locally.
app.MapPost("/debug/hash-plaintext-passwords", async (MealMindDbContext db) =>
{
    // Find users that likely still store plaintext (simple heuristic: short value or no typical hash prefix)
    var candidates = await db.Gebruikers
        .Where(g => !string.IsNullOrEmpty(g.GebruikerPasswordHash))
        .ToListAsync();

    if (!candidates.Any())
        return Results.Ok("No users found with a password value to convert.");

    var hasher = new PasswordHasher<Gebruiker>();
    var converted = 0;

    foreach (var user in candidates)
    {
        var current = user.GebruikerPasswordHash ?? "";
        // Heuristic: if value looks like a hashed value produced by PasswordHasher it usually starts with "AQAAAA" or "$"
        // Adjust if you have different hash formats. We only convert short/plain values here.
        var looksHashed = current.Length > 20 && (current.StartsWith("AQAAAA") || current.StartsWith("$"));
        if (looksHashed)
            continue;

        // Treat current value as plaintext password and replace with hashed password.
        user.GebruikerPasswordHash = hasher.HashPassword(user, current);
        converted++;
    }

    if (converted > 0)
        await db.SaveChangesAsync();

    return Results.Ok(new { Converted = converted, Checked = candidates.Count });
});

app.Run();

// DTOs
record RegistrationRequest(string Voornaam, string? Achternaam, string? Username, string Email, string Password);
record LoginRequest(string Identifier, string Password);

record IngredientRequest(string Naam, string Hoeveelheid);
record ReceptRequest(string Naam, string Beschrijving, List<IngredientRequest> Ingredienten, List<int> CategorieIds);
record PlanningUpdate(int ReceptId);
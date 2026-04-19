using System;
using System.Collections.Generic;

namespace MealMind.Models;

public partial class Recept
{
    public int IdRecept { get; set; }

    public string ReceptNaam { get; set; } = null!;

    public string ReceptBeshrijving { get; set; } = null!;

    public virtual ICollection<Gebruikerkiestrecept> Gebruikerkiestrecepts { get; set; } = new List<Gebruikerkiestrecept>();

    public virtual ICollection<ReceptHasIngredient> ReceptHasIngredients { get; set; } = new List<ReceptHasIngredient>();

    public virtual ICollection<Categorie> FkCategories { get; set; } = new List<Categorie>();
}

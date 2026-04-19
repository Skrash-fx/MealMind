using System;
using System.Collections.Generic;

namespace MealMind.Models;

public partial class Ingredient
{
    public int IdIngredient { get; set; }

    public string IngredientNaam { get; set; } = null!;

    public virtual ICollection<ReceptHasIngredient> ReceptHasIngredients { get; set; } = new List<ReceptHasIngredient>();
}

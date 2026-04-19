using System;
using System.Collections.Generic;

namespace MealMind.Models;

public partial class Categorie
{
    public int IdCategorie { get; set; }

    public string CategorieNaam { get; set; } = null!;

    public virtual ICollection<Recept> FkRecepts { get; set; } = new List<Recept>();
}

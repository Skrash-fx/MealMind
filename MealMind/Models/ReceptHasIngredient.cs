using System;
using System.Collections.Generic;

namespace MealMind.Models;

public partial class ReceptHasIngredient
{
    public int FkRecept { get; set; }

    public int FkIngredient { get; set; }

    public string HoeveelheidIngredient { get; set; } = null!;

    public virtual Ingredient FkIngredientNavigation { get; set; } = null!;

    public virtual Recept FkReceptNavigation { get; set; } = null!;
}

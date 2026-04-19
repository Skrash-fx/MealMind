using System;
using System.Collections.Generic;

namespace MealMind.Models;

public partial class Gebruikerkiestrecept
{
    public int FkRecept { get; set; }

    public int FkGebruiker { get; set; }

    public DateOnly? Datum { get; set; }

    public virtual Gebruiker FkGebruikerNavigation { get; set; } = null!;

    public virtual Recept FkReceptNavigation { get; set; } = null!;
}

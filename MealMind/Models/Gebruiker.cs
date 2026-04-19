using System;
using System.Collections.Generic;

namespace MealMind.Models;

public partial class Gebruiker
{
    public int IdGebruiker { get; set; }

    // Keep only the columns that exist in your DB
    public string? GebruikerUsername { get; set; }
    public string? GebruikerEmail { get; set; }
    public string? GebruikerPasswordHash { get; set; }

    public virtual ICollection<Gebruikerkiestrecept> Gebruikerkiestrecepts { get; set; } = new List<Gebruikerkiestrecept>();
}

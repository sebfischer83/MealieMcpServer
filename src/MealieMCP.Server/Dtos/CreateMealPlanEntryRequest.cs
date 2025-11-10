using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MealieMCP.Server.Dtos;

/// <summary>
/// Payload used to create a new household meal plan entry.
/// </summary>
public sealed class CreateMealPlanEntryRequest
{
    [Required]
    [Description("Datum des Eintrags im Format YYYY-MM-DD.")]
    public DateOnly Date { get; init; }

    [Description("Art des Eintrags, z. B. Frühstück oder Mittagessen.")]
    public MealPlanEntryType EntryType { get; init; } = MealPlanEntryType.Breakfast;

    [Description("Optionaler Titel, der im Wochenplan angezeigt wird.")]
    public string? Title { get; init; }

    [Description("Optionaler Freitext für zusätzliche Hinweise.")]
    public string? Text { get; init; }

    [Description("Optionale ID eines Rezepts, das dem Eintrag zugeordnet wird.")]
    public Guid? RecipeId { get; init; }
}

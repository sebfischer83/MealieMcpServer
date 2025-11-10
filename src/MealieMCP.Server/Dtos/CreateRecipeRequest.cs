using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MealieMCP.Server.Dtos;

/// <summary>
/// Payload used to create a new recipe via the Mealie API.
/// </summary>
public sealed class CreateRecipeRequest
{
    /// <summary>
    /// Name of the recipe.
    /// </summary>
    [Required]
    [Description("Name des neuen Rezepts.")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Optional additional properties that will be forwarded to the API.
    /// </summary>
    [Description("Optionale zusätzliche Eigenschaften als Schlüssel/Wert-Paare, z. B. {\"description\": \"Text\"}.")]
    public Dictionary<string, object?>? AdditionalProperties { get; init; }
}

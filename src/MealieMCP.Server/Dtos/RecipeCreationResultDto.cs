namespace MealieMCP.Server.Dtos;

/// <summary>
/// Represents the identifier returned after creating a recipe.
/// </summary>
/// <param name="RecipeId">Identifier of the newly created recipe (typically the slug).</param>
public sealed record RecipeCreationResultDto(string? RecipeId);

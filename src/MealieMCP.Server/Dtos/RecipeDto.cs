using System;
using System.Collections.Generic;

namespace MealieMCP.Server.Dtos;

/// <summary>
/// Simplified representation of a recipe returned by the Mealie API.
/// </summary>
public sealed record RecipeDto
{
    /// <summary>
    /// Unique identifier of the recipe.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Human readable name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Optional description of the recipe.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Slug used to build URLs.
    /// </summary>
    public string? Slug { get; init; }

    /// <summary>
    /// Average user rating.
    /// </summary>
    public double? Rating { get; init; }

    /// <summary>
    /// Number of servings.
    /// </summary>
    public double? Servings { get; init; }

    /// <summary>
    /// When the recipe has been created.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>
    /// When the recipe has been updated the last time.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// Tags associated with the recipe.
    /// </summary>
    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();
}

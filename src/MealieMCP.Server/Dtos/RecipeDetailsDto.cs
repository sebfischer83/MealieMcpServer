using System;
using System.Collections.Generic;

namespace MealieMCP.Server.Dtos;

/// <summary>
/// Detailed representation of a recipe with instructions and ingredients.
/// </summary>
public sealed record RecipeDetailsDto
{
    public string? Id { get; init; }

    public string? Slug { get; init; }

    public string? Name { get; init; }

    public string? Description { get; init; }

    public double? Rating { get; init; }

    public double? Servings { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? UpdatedAt { get; init; }

    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<string> Ingredients { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<string> Instructions { get; init; } = Array.Empty<string>();
}

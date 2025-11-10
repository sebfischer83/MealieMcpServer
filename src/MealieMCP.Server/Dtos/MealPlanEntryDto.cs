using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MealieMCP.Server.Dtos;

/// <summary>
/// Represents a single entry within the household meal plan.
/// </summary>
public sealed record MealPlanEntryDto
{
    public int? Id { get; init; }

    public DateOnly? Date { get; init; }

    public MealPlanEntryType? EntryType { get; init; }

    public string? Title { get; init; }

    public string? Text { get; init; }

    public Guid? RecipeId { get; init; }

    public Guid? HouseholdId { get; init; }

    public Guid? GroupId { get; init; }

    public Guid? UserId { get; init; }

    public RecipeDto? Recipe { get; init; }
}

/// <summary>
/// Type of meal plan entry supported by Mealie.
/// </summary>
public enum MealPlanEntryType
{
    [Description("Frühstück")]
    Breakfast,
    [Description("Mittagessen")]
    Lunch,
    [Description("Abendessen")]
    Dinner,
    [Description("Beilage oder Snack")]
    Side
}

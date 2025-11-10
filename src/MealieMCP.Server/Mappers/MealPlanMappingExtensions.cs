using System;
using System.Collections.Generic;
using System.Linq;
using MealiMCP.Server.Api.Models;
using MealieMCP.Server.Dtos;

namespace MealieMCP.Server.Mappers;

public static class MealPlanMappingExtensions
{
    public static MealPlanEntryDto ToDto(this ReadPlanEntry entry)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        var recipeDto = entry.Recipe?.ToDto();
        var recipeId = MappingHelpers.GetGuid(entry.RecipeId?.Guid, entry.RecipeId?.ReadPlanEntryRecipeIdMember1?.AdditionalData, "id", "recipeId");

        if (!recipeId.HasValue && Guid.TryParse(recipeDto?.Id, out var parsedFromRecipe))
        {
            recipeId = parsedFromRecipe;
        }

        return new MealPlanEntryDto
        {
            Id = entry.Id,
            Date = MappingHelpers.GetDate(entry.Date, null),
            EntryType = entry.EntryType.ToMealPlanEntryType(),
            Title = entry.Title,
            Text = entry.Text,
            RecipeId = recipeId,
            HouseholdId = entry.HouseholdId,
            GroupId = entry.GroupId,
            UserId = entry.UserId,
            Recipe = recipeDto
        };
    }

    public static PagedResultDto<MealPlanEntryDto> ToDto(this PlanEntryPagination pagination)
    {
        if (pagination == null)
        {
            throw new ArgumentNullException(nameof(pagination));
        }

        var items = pagination.Items?
            .Where(static item => item != null)
            .Select(static item => item!.ToDto())
            .ToList() ?? new List<MealPlanEntryDto>();

        return new PagedResultDto<MealPlanEntryDto>(
            items,
            pagination.Page ?? 0,
            pagination.PerPage ?? 0,
            pagination.Total ?? 0,
            pagination.TotalPages ?? 0,
            ExtractLink(pagination.Next),
            ExtractLink(pagination.Previous));
    }

    public static MealPlanEntryType? ToMealPlanEntryType(this PlanEntryType? value)
        => value switch
        {
            PlanEntryType.Breakfast => MealPlanEntryType.Breakfast,
            PlanEntryType.Lunch => MealPlanEntryType.Lunch,
            PlanEntryType.Dinner => MealPlanEntryType.Dinner,
            PlanEntryType.Side => MealPlanEntryType.Side,
            _ => null
        };

    public static PlanEntryType ToApiType(this MealPlanEntryType value)
        => value switch
        {
            MealPlanEntryType.Breakfast => PlanEntryType.Breakfast,
            MealPlanEntryType.Lunch => PlanEntryType.Lunch,
            MealPlanEntryType.Dinner => PlanEntryType.Dinner,
            MealPlanEntryType.Side => PlanEntryType.Side,
            _ => PlanEntryType.Breakfast
        };

    private static string? ExtractLink(PlanEntryPagination.PlanEntryPagination_next? link)
    {
        if (link == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(link.String))
        {
            return link.String;
        }

        return MappingHelpers.ExtractStringFromAdditionalData(link.PlanEntryPaginationNextMember1?.AdditionalData, "href", "url");
    }

    private static string? ExtractLink(PlanEntryPagination.PlanEntryPagination_previous? link)
    {
        if (link == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(link.String))
        {
            return link.String;
        }

        return MappingHelpers.ExtractStringFromAdditionalData(link.PlanEntryPaginationPreviousMember1?.AdditionalData, "href", "url");
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using MealiMCP.Server.Api.Models;
using MealieMCP.Server.Dtos;

namespace MealieMCP.Server.Mappers;

public static class RecipeMappingExtensions
{
    public static RecipeDto ToDto(this RecipeSummary recipe)
    {
        if (recipe == null)
        {
            throw new ArgumentNullException(nameof(recipe));
        }

        var tags = recipe.Tags != null
            ? recipe.Tags
                .Select(tag => MappingHelpers.ExtractStringFromAdditionalData(tag?.AdditionalData, "name", "label", "title", "slug"))
                .Where(static tag => !string.IsNullOrWhiteSpace(tag))
                .Select(static tag => tag!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : Array.Empty<string>();

        return new RecipeDto
        {
            Id = MappingHelpers.GetStringValue(recipe.Id, static x => x.String, static x => x.RecipeSummaryIdMember1?.AdditionalData),
            Name = MappingHelpers.GetStringValue(recipe.Name, static x => x.String, static x => x.RecipeSummaryNameMember1?.AdditionalData),
            Description = MappingHelpers.GetStringValue(recipe.Description, static x => x.String, static x => x.RecipeSummaryDescriptionMember1?.AdditionalData),
            Slug = recipe.Slug,
            Rating = recipe.Rating?.Double,
            Servings = recipe.RecipeServings,
            CreatedAt = MappingHelpers.GetDateTime(recipe.CreatedAt?.DateTimeOffset, recipe.CreatedAt?.RecipeSummaryCreatedAtMember1?.AdditionalData),
            UpdatedAt = MappingHelpers.GetDateTime(recipe.UpdatedAt?.DateTimeOffset, recipe.UpdatedAt?.RecipeSummaryUpdatedAtMember1?.AdditionalData),
            Tags = tags
        };
    }

    public static PagedResultDto<RecipeDto> ToDto(this PaginationBase_RecipeSummary_ source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var items = source.Items?
            .Where(static item => item != null)
            .Select(static item => item!.ToDto())
            .ToList() ?? new List<RecipeDto>();

        return new PagedResultDto<RecipeDto>(
            items,
            source.Page ?? 0,
            source.PerPage ?? 0,
            source.Total ?? 0,
            source.TotalPages ?? 0,
            ExtractLink(source.Next),
            ExtractLink(source.Previous));
    }

    public static RecipeDetailsDto ToDetailsDto(this RecipeOutput recipe)
    {
        if (recipe == null)
        {
            throw new ArgumentNullException(nameof(recipe));
        }

        var tags = recipe.Tags != null
            ? recipe.Tags
                .Select(tag => MappingHelpers.ExtractStringFromAdditionalData(tag?.AdditionalData, "name", "label", "title", "slug"))
                .Where(static tag => !string.IsNullOrWhiteSpace(tag))
                .Select(static tag => tag!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : Array.Empty<string>();

        var ingredients = recipe.RecipeIngredient != null
            ? recipe.RecipeIngredient
                .Select(static ingredient =>
                    !string.IsNullOrWhiteSpace(ingredient?.Display)
                        ? ingredient!.Display
                        : MappingHelpers.ExtractStringFromAdditionalData(ingredient?.AdditionalData, "display", "originalText", "text", "label"))
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Select(static value => value!.Trim())
                .ToArray()
            : Array.Empty<string>();

        var instructions = recipe.RecipeInstructions != null
            ? recipe.RecipeInstructions
                .Select(static step => MappingHelpers.ExtractStringFromAdditionalData(step?.AdditionalData, "text", "instruction", "description"))
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Select(static value => value!.Trim())
                .ToArray()
            : Array.Empty<string>();

        var rating = MappingHelpers.GetDouble(recipe.Rating?.Double, recipe.Rating?.RecipeOutputRatingMember1?.AdditionalData, "rating", "value", "score");
        var servings = MappingHelpers.GetDouble(recipe.RecipeYieldQuantity, recipe.RecipeYield?.RecipeOutputRecipeYieldMember1?.AdditionalData, "quantity", "servings", "value");

        return new RecipeDetailsDto
        {
            Id = MappingHelpers.GetStringValue(recipe.Id, static x => x.String, static x => x.RecipeOutputIdMember1?.AdditionalData),
            Slug = recipe.Slug,
            Name = MappingHelpers.GetStringValue(recipe.Name, static x => x.String, static x => x.RecipeOutputNameMember1?.AdditionalData),
            Description = MappingHelpers.GetStringValue(recipe.Description, static x => x.String, static x => x.RecipeOutputDescriptionMember1?.AdditionalData),
            Rating = rating,
            Servings = servings,
            CreatedAt = MappingHelpers.GetDateTime(recipe.CreatedAt?.DateTimeOffset, recipe.CreatedAt?.RecipeOutputCreatedAtMember1?.AdditionalData),
            UpdatedAt = MappingHelpers.GetDateTime(recipe.UpdatedAt?.DateTimeOffset, recipe.UpdatedAt?.RecipeOutputUpdatedAtMember1?.AdditionalData),
            Tags = tags,
            Ingredients = ingredients,
            Instructions = instructions
        };
    }

    private static string? ExtractLink(PaginationBase_RecipeSummary_.PaginationBase_RecipeSummary__next? link)
    {
        if (link == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(link.String))
        {
            return link.String;
        }

        return MappingHelpers.ExtractStringFromAdditionalData(link.PaginationBaseRecipeSummaryNextMember1?.AdditionalData, "href", "url");
    }

    private static string? ExtractLink(PaginationBase_RecipeSummary_.PaginationBase_RecipeSummary__previous? link)
    {
        if (link == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(link.String))
        {
            return link.String;
        }

        return MappingHelpers.ExtractStringFromAdditionalData(link.PaginationBaseRecipeSummaryPreviousMember1?.AdditionalData, "href", "url");
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
                .Select(tag => ExtractStringFromAdditionalData(tag?.AdditionalData, "name", "label", "title", "slug"))
                .Where(static tag => !string.IsNullOrWhiteSpace(tag))
                .Select(static tag => tag!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : Array.Empty<string>();

        return new RecipeDto
        {
            Id = GetStringValue(recipe.Id, static x => x.String, static x => x.RecipeSummaryIdMember1?.AdditionalData),
            Name = GetStringValue(recipe.Name, static x => x.String, static x => x.RecipeSummaryNameMember1?.AdditionalData),
            Description = GetStringValue(recipe.Description, static x => x.String, static x => x.RecipeSummaryDescriptionMember1?.AdditionalData),
            Slug = recipe.Slug,
            Rating = recipe.Rating?.Double,
            Servings = recipe.RecipeServings,
            CreatedAt = GetDateTime(recipe.CreatedAt),
            UpdatedAt = GetDateTime(recipe.UpdatedAt),
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

        return ExtractStringFromAdditionalData(link.PaginationBaseRecipeSummaryNextMember1?.AdditionalData, "href", "url");
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

        return ExtractStringFromAdditionalData(link.PaginationBaseRecipeSummaryPreviousMember1?.AdditionalData, "href", "url");
    }

    private static DateTimeOffset? GetDateTime(RecipeSummary.RecipeSummary_createdAt? value)
        => GetDateTime(value?.DateTimeOffset, value?.RecipeSummaryCreatedAtMember1?.AdditionalData);

    private static DateTimeOffset? GetDateTime(RecipeSummary.RecipeSummary_updatedAt? value)
        => GetDateTime(value?.DateTimeOffset, value?.RecipeSummaryUpdatedAtMember1?.AdditionalData);

    private static DateTimeOffset? GetDateTime(DateTimeOffset? directValue, IDictionary<string, object>? additionalData)
    {
        if (directValue.HasValue)
        {
            return directValue.Value;
        }

        var fallback = ExtractStringFromAdditionalData(additionalData, "iso", "value", "timestamp");
        if (string.IsNullOrWhiteSpace(fallback))
        {
            return null;
        }

        return DateTimeOffset.TryParse(fallback, out var parsed) ? parsed : null;
    }

    private static string? GetStringValue<TWrapper>(
        TWrapper? wrapper,
        Func<TWrapper, string?> stringSelector,
        Func<TWrapper, IDictionary<string, object>?> additionalSelector)
        where TWrapper : class
    {
        if (wrapper == null)
        {
            return null;
        }

        var direct = stringSelector(wrapper);
        if (!string.IsNullOrWhiteSpace(direct))
        {
            return direct;
        }

        return ExtractStringFromAdditionalData(additionalSelector(wrapper));
    }

    private static string? ExtractStringFromAdditionalData(IDictionary<string, object>? additionalData, params string[] preferredKeys)
    {
        if (additionalData == null || additionalData.Count == 0)
        {
            return null;
        }

        foreach (var key in preferredKeys)
        {
            if (additionalData.TryGetValue(key, out var value))
            {
                var text = ConvertToString(value);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }

        foreach (var value in additionalData.Values)
        {
            var text = ConvertToString(value);
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text;
            }
        }

        return null;
    }

    private static string? ConvertToString(object? value)
    {
        switch (value)
        {
            case null:
                return null;
            case string text:
                return text;
            case JsonElement jsonElement:
                return ExtractFromJsonElement(jsonElement);
            case IDictionary<string, object> nestedDictionary:
                return ExtractStringFromAdditionalData(nestedDictionary);
            case IEnumerable enumerable:
                foreach (var item in enumerable)
                {
                    var text = ConvertToString(item);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
                return null;
            default:
                return value.ToString();
        }
    }

    private static string? ExtractFromJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                return element.ToString();
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    var text = ExtractFromJsonElement(item);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
                return null;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var text = ExtractFromJsonElement(property.Value);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
                return null;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                return element.ToString();
        }
    }
}

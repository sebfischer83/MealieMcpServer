using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Microsoft.Kiota.Abstractions;

namespace MealieMCP.Server.Mappers;

internal static class MappingHelpers
{
    public static string? GetStringValue<TWrapper>(
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

    public static string? ExtractStringFromAdditionalData(
        IDictionary<string, object>? additionalData,
        params string[] preferredKeys)
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

    public static DateTimeOffset? GetDateTime(
        DateTimeOffset? directValue,
        IDictionary<string, object>? additionalData)
    {
        if (directValue.HasValue)
        {
            return directValue.Value;
        }

        var fallback = ExtractStringFromAdditionalData(additionalData, "iso", "value", "timestamp", "date");
        if (string.IsNullOrWhiteSpace(fallback))
        {
            return null;
        }

        return DateTimeOffset.TryParse(fallback, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : null;
    }

    public static DateOnly? GetDate(Date? date, IDictionary<string, object>? additionalData)
    {
        if (date.HasValue)
        {
            return new DateOnly(date.Value.Year, date.Value.Month, date.Value.Day);
        }

        var fallback = ExtractStringFromAdditionalData(additionalData, "date", "value", "iso");
        if (string.IsNullOrWhiteSpace(fallback))
        {
            return null;
        }

        return DateOnly.TryParse(fallback, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    public static double? GetDouble(
        double? directValue,
        IDictionary<string, object>? additionalData,
        params string[] preferredKeys)
    {
        if (directValue.HasValue)
        {
            return directValue.Value;
        }

        var text = ExtractStringFromAdditionalData(additionalData, preferredKeys);
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    public static Guid? GetGuid(Guid? directValue, IDictionary<string, object>? additionalData, params string[] preferredKeys)
    {
        if (directValue.HasValue)
        {
            return directValue.Value;
        }

        var text = ExtractStringFromAdditionalData(additionalData, preferredKeys);
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return Guid.TryParse(text, out var parsed) ? parsed : null;
    }

    public static string? ConvertToString(object? value)
    {
        switch (value)
        {
            case null:
                return null;
            case string text:
                return text;
            case JsonElement jsonElement:
                return ExtractFromJsonElement(jsonElement);
            case IDictionary<string, object> dictionary:
                return ExtractStringFromAdditionalData(dictionary);
            case IFormattable formattable:
                return formattable.ToString(null, CultureInfo.InvariantCulture);
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
    }

    private static string? ExtractFromJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
                return element.GetRawText();
            case JsonValueKind.True:
                return bool.TrueString;
            case JsonValueKind.False:
                return bool.FalseString;
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
            default:
                return null;
        }
    }
}

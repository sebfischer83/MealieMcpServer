using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using MealiMCP.Server.Api;
using MealiMCP.Server.Api.Models;
using MealieMCP.Server.Dtos;
using MealieMCP.Server.Mappers;
using Microsoft.Kiota.Abstractions;
using ModelContextProtocol.Server;

namespace MealieMCP.Server.Tools
{
    [McpServerToolType]
    public sealed class ReceipeTools
    {
        public ReceipeTools(ApiClient apiClient)
        {
            ApiClient = apiClient;
        }

        public ApiClient ApiClient { get; }

        [McpServerTool, Description("Sucht nach Rezepten basierend auf der Suchanfrage.")]
        public async Task<RecipeDto> SearchReceipeAsync(string query)
        {
            var receipes = await ApiClient.Api.Recipes.GetAsync(c =>
            {
                c.QueryParameters.Search = query;
            });

            if (receipes?.Items == null)
            {
                throw new Exception("Keine Ergebnisse erhalten.");
            }

            var recipe = receipes.Items.FirstOrDefault();
            if (recipe == null)
            {
                throw new Exception("Kein Rezept gefunden.");
            }

            return recipe.ToDto();
        }

        [McpServerTool, Description("Sucht nach Rezepten und liefert eine einzelne Seite mit Paging-Informationen. Übergib optional page, perPage oder paginationSeed.")]
        public async Task<PagedResultDto<RecipeDto>> SearchReceipePageAsync(string query, int? page = null, int? perPage = null, string? paginationSeed = null)
        {
            var pageResult = await ApiClient.Api.Recipes.GetAsync(c =>
            {
                c.QueryParameters.Search = query;
                if (page.HasValue) c.QueryParameters.Page = page.Value;
                if (perPage.HasValue) c.QueryParameters.PerPage = perPage.Value;
                if (!string.IsNullOrWhiteSpace(paginationSeed)) c.QueryParameters.PaginationSeed = paginationSeed;
            });

            if (pageResult == null)
            {
                throw new Exception("Keine Ergebnisse erhalten.");
            }

            return pageResult.ToDto();
        }

        [McpServerTool, Description("Lädt die vollständigen Details eines Rezepts über den Slug.")]
        public async Task<RecipeDetailsDto> GetRecipeDetailsAsync([Description("Slug oder ID des Rezepts")] string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new ArgumentException("Slug darf nicht leer sein.", nameof(slug));
            }

            var recipe = await ApiClient.Api.Recipes[slug].GetAsync();
            if (recipe == null)
            {
                throw new Exception($"Rezept '{slug}' wurde nicht gefunden.");
            }

            return recipe.ToDetailsDto();
        }

        [McpServerTool, Description("Legt ein neues Rezept an.")]
        public async Task<RecipeCreationResultDto> CreateRecipeAsync(CreateRecipeRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Der Name des Rezepts wird benötigt.", nameof(request));
            }

            var payload = new CreateRecipe
            {
                Name = request.Name
            };

            if (request.AdditionalProperties != null)
            {
                foreach (var (key, value) in request.AdditionalProperties)
                {
                    if (string.IsNullOrWhiteSpace(key) || value is null)
                    {
                        continue;
                    }

                    payload.AdditionalData[key] = value;
                }
            }

            var identifier = await ApiClient.Api.Recipes.PostAsync(payload)
                ?? throw new Exception("Die API hat keine Antwort geliefert.");

            return new RecipeCreationResultDto(identifier);
        }

        [McpServerTool, Description("Listet Wochenplan-Einträge der aktuellen Haushalts auf.")]
        public async Task<PagedResultDto<MealPlanEntryDto>> GetMealPlanPageAsync(
            [Description("Optionales Startdatum (YYYY-MM-DD)")] DateOnly? startDate = null,
            [Description("Optionales Enddatum (YYYY-MM-DD)")] DateOnly? endDate = null,
            [Description("Seitennummer (1-basiert)")] int? page = null,
            [Description("Anzahl Einträge pro Seite")] int? perPage = null,
            [Description("Seed für zufällige Paginierung")] string? paginationSeed = null)
        {
            var response = await ApiClient.Api.Households.Mealplans.GetAsync(options =>
            {
                if (startDate.HasValue)
                {
                    options.QueryParameters.StartDate = startDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                if (endDate.HasValue)
                {
                    options.QueryParameters.EndDate = endDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                if (page.HasValue)
                {
                    options.QueryParameters.Page = page.Value;
                }

                if (perPage.HasValue)
                {
                    options.QueryParameters.PerPage = perPage.Value;
                }

                if (!string.IsNullOrWhiteSpace(paginationSeed))
                {
                    options.QueryParameters.PaginationSeed = paginationSeed;
                }
            });

            if (response == null)
            {
                throw new Exception("Keine Wochenplan-Daten erhalten.");
            }

            return response.ToDto();
        }

        [McpServerTool, Description("Fügt dem Wochenplan einen neuen Eintrag hinzu.")]
        public async Task<MealPlanEntryDto> CreateMealPlanEntryAsync(CreateMealPlanEntryRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var payload = new CreatePlanEntry
            {
                Date = new Date(request.Date.Year, request.Date.Month, request.Date.Day),
                EntryType = request.EntryType.ToApiType(),
                Title = request.Title,
                Text = request.Text
            };

            if (request.RecipeId.HasValue)
            {
                payload.RecipeId = new CreatePlanEntry.CreatePlanEntry_recipeId
                {
                    Guid = request.RecipeId.Value
                };
            }

            var created = await ApiClient.Api.Households.Mealplans.PostAsync(payload)
                ?? throw new Exception("Der Wochenplan konnte nicht aktualisiert werden.");

            return created.ToDto();
        }

        [McpServerTool, Description("Echo: gibt die Nachricht zurück.")]
        public static string Echo([Description("Die Nachricht")] string message)
         => $"echo: {message}";

        [McpServerTool(Name = "sum"), Description("Addiert zwei Zahlen.")]
        public static int Sum(int a, int b) => a + b;
    }
}

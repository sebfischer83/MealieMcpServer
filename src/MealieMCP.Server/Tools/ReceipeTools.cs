using System;
using System.ComponentModel;
using System.Linq;
using MealiMCP.Server.Api;
using MealieMCP.Server.Dtos;
using MealieMCP.Server.Mappers;
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

        [McpServerTool, Description("Echo: gibt die Nachricht zurück.")]
        public static string Echo([Description("Die Nachricht")] string message)
         => $"echo: {message}";

        [McpServerTool(Name = "sum"), Description("Addiert zwei Zahlen.")]
        public static int Sum(int a, int b) => a + b;
    }
}

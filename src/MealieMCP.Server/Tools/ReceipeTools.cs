using MealiMCP.Server.Api;
using MealiMCP.Server.Api.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;

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
        public async Task<RecipeSummary> SearchReceipeAsync(string query)
        {
            var receipes = await ApiClient.Api.Recipes.GetAsync(c =>
            {
                c.QueryParameters.Search = query;
            });
                        
            return receipes.Items?.FirstOrDefault() ?? throw new Exception("Kein Rezept gefunden.");
        }

        [McpServerTool, Description("Sucht nach Rezepten und liefert eine einzelne Seite mit Paging-Informationen. Übergib optional page, perPage oder paginationSeed.")]
        public async Task<PaginationBase_RecipeSummary_> SearchReceipePageAsync(string query, int? page = null, int? perPage = null, string? paginationSeed = null)
        {
            var pageResult = await ApiClient.Api.Recipes.GetAsync(c =>
            {
                c.QueryParameters.Search = query;
                if (page.HasValue) c.QueryParameters.Page = page.Value;
                if (perPage.HasValue) c.QueryParameters.PerPage = perPage.Value;
                if (!string.IsNullOrWhiteSpace(paginationSeed)) c.QueryParameters.PaginationSeed = paginationSeed;
            });

            if (pageResult == null) throw new Exception("Keine Ergebnisse erhalten.");
            return pageResult;
        }

        [McpServerTool, Description("Echo: gibt die Nachricht zurück.")]
        public static string Echo([Description("Die Nachricht")] string message)
     => $"echo: {message}";

        [McpServerTool(Name = "sum"), Description("Addiert zwei Zahlen.")]
        public static int Sum(int a, int b) => a + b;
    }
}

namespace MealieMCP.Server.Config
{
    // Einstellungsobjekt für die Mealie API-Konfiguration
    public class MealieSettings
    {
        // Basis-URL des Mealie-Servers, z.B. "http://mealie:9000"
        public string Url { get; set; } = string.Empty;

        // Optionaler API-Schlüssel
        public string ApiKey { get; set; } = string.Empty;
    }
}

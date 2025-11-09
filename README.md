Mealie-MCP
==========

Kurzbeschreibung
----------------
Dieses Repository enthält den MCP‑Server für Mealie ("Mealie MCP"). Der Server stellt Tool‑Methoden für die Model Context Protocol (MCP) Oberfläche bereit, z. B. Rezeptsuche mit Paging.

Wichtigste Dateien
- `src/MealieMCP.Server` – ASP.NET Core Serverprojekt
- `src/MealieMCP.Server/Tools/ReceipeTools.cs` – MCP‑Tool‑Methoden (z. B. `SearchReceipeAsync`, `SearchReceipePageAsync`)

Voraussetzungen
- .NET 9 SDK
- Optional: Docker & Docker Compose (für Containerbetrieb)

Build & Run (lokal)
1. Projekt wiederherstellen und bauen:
   - `dotnet restore` (im Repo‑Root oder `src/MealieMCP.Server`)
   - `dotnet build src/MealieMCP.Server`
2. Server starten:
   - `dotnet run --project src/MealieMCP.Server`

Authentifizierung
-----------------
- Die HTTP‑Schnittstelle der MCP‑Tools kann mit API‑Tokens geschützt werden.
- Konfiguriere dazu im `appsettings.json` (oder via Umgebungsvariablen, z. B. `MealiMcp__ApiTokenAuthentication__Tokens__0`) eine Liste gültiger Tokens.
- Clients senden das Token entweder als `Authorization: Bearer <token>`, `X-Api-Token: <token>` oder `?access_token=<token>`.
- Sind keine Tokens konfiguriert, bleibt der Zugriff wie bisher offen.

Docker
- Zum Starten mit Docker Compose: `docker compose up --build`

MCP Tools & Paging
- Die Klasse `ReceipeTools` stellt MCP‑Tools bereit, z. B.:
  - `SearchReceipeAsync(string query)` — gibt das erste gefundene `RecipeSummary` zurück.
  - `SearchReceipePageAsync(string query, int? page = null, int? perPage = null, string? paginationSeed = null)` — liefert eine einzelne Seite mit Paging‑Metadaten (`PaginationBase_RecipeSummary_`).

Beispiele
- Seite 2 anfragen (C# Aufruf innerhalb des Servers / Tests):
  - `await receipeTools.SearchReceipePageAsync("suppe", page: 2);`
- Cursor‑Paging mit `paginationSeed`:
  - Verwende das Feld `Next` im `PaginationBase_RecipeSummary_`‑Objekt, um den nächsten `paginationSeed` zu erhalten und an `SearchReceipePageAsync(..., paginationSeed: "...")` zu übergeben.

Hinweise
- Achte auf Rate‑Limits und Timeouts wenn mehrere Seiten hintereinander abgefragt werden.
- Für vollständige Aggregation kann eine zusätzliche Methode implementiert werden, die intern alle Seiten zusammenführt.

Contributing
- Änderungen per Pull Request. Achte auf Coding‑Standards und Unit Tests.

Lizenz
- (Platzhalter) Bitte Lizenz in `LICENSE` hinzufügen.

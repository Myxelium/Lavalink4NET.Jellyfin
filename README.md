# Lavalink4NET.Jellyfin

[![NuGet](https://img.shields.io/nuget/v/Lavalink4NET.Jellyfin.svg)](https://www.nuget.org/packages/Lavalink4NET.Jellyfin)
[![License](https://img.shields.io/github/license/Myxelium/Lavalink4NET.Jellyfin)](LICENSE)

Extends [Lavalink4NET](https://github.com/angelobreuer/Lavalink4NET) with Jellyfin search support (`jfsearch:`) and utilities for custom search modes. Works with the [Jellylink](https://github.com/Myxelium/Jellylink) Lavalink plugin.

## Table of Contents

- [Installation](#installation)
- [Features](#features)
- [Quick Start](#quick-start)
- [API Reference](#api-reference)
  - [JellyfinSearchMode](#jellyfinearchmode)
  - [SearchQueryParser](#searchqueryparser)
  - [SearchQueryResult](#searchqueryresult)
  - [SearchSource Enum](#searchsource-enum)
- [Examples](#examples)
  - [Basic Usage](#basic-usage)
  - [Setting Default Search Provider](#setting-default-search-provider)
  - [Using Extended Parse Results](#using-extended-parse-results)
  - [Registering Custom Prefixes](#registering-custom-prefixes)
  - [Discord Bot Integration](#discord-bot-integration)
- [Supported Prefixes](#supported-prefixes)
- [Requirements](#requirements)
- [License](#license)

---

## Installation

```bash
dotnet add package Lavalink4NET.Jellyfin
```

Or via the NuGet Package Manager:

```powershell
Install-Package Lavalink4NET.Jellyfin
```

---

## Features

- 🎵 **Jellyfin Search Mode** - Search your Jellyfin library directly via Lavalink
- 🔧 **Custom Search Modes** - Create your own search prefixes for any source
- 📝 **Smart Query Parser** - Automatically detect and parse search prefixes from user input
- 🎯 **SearchSource Enum** - Clearly identify which platform a search targets
- ✨ **Extension Methods** - Fluent API for creating `TrackLoadOptions`
- 🔗 **URL Detection** - Automatically handles direct URLs vs search queries

---

## Quick Start

```csharp
using Lavalink4NET.Jellyfin;
using Lavalink4NET.Rest.Entities.Tracks;

// Parse user input - automatically detects prefixes
var (searchMode, cleanQuery) = SearchQueryParser.Parse("jfsearch:Bohemian Rhapsody");

// Use with Lavalink4NET
var options = new TrackLoadOptions { SearchMode = searchMode };
var tracks = await audioService.Tracks.LoadTracksAsync(cleanQuery, options);
```

---

## API Reference

### JellyfinSearchMode

Static class providing Jellyfin search mode and utilities for creating custom search modes.

```csharp
public static class JellyfinSearchMode
{
    // Pre-configured Jellyfin search mode (jfsearch:)
    public static TrackSearchMode Jellyfin { get; }
    
    // Create a custom search mode with any prefix
    public static TrackSearchMode Custom(string prefix);
}
```

#### Examples

```csharp
// Use the built-in Jellyfin search mode
var jellyfinMode = JellyfinSearchMode.Jellyfin;

// Create a custom search mode for your own source
var myCustomMode = JellyfinSearchMode.Custom("mysource");
```

---

### SearchQueryParser

Static class for parsing search queries and detecting prefixes.

#### Methods

| Method | Description |
|--------|-------------|
| `Parse(query, defaultMode)` | Parses query and returns `ParsedSearchQuery` (simple) |
| `ParseExtended(query, defaultMode)` | Parses query and returns `SearchQueryResult` (detailed) |
| `RegisterPrefix(prefix, mode, source)` | Registers a custom prefix |
| `UnregisterPrefix(prefix)` | Removes a registered prefix |
| `IsPrefixRegistered(prefix)` | Checks if a prefix is registered |
| `GetRegisteredPrefixes()` | Gets all registered prefixes |
| `TryGetSearchMode(prefix, out mode, out source)` | Tries to get mode and source for a prefix |

#### Parse Method

```csharp
public static ParsedSearchQuery Parse(
    string query, 
    TrackSearchMode defaultMode = default
);
```

**Parameters:**
- `query` - The user's search input (may include a prefix like `jfsearch:`)
- `defaultMode` - The search mode to use when no prefix is detected (defaults to YouTube)

**Returns:** `ParsedSearchQuery` with `SearchMode` and `Query` properties

#### ParseExtended Method

```csharp
public static SearchQueryResult ParseExtended(
    string query, 
    TrackSearchMode defaultMode = default
);
```

**Returns:** `SearchQueryResult` with detailed information about the parsed query

---

### SearchQueryResult

A detailed result structure returned by `ParseExtended()`.

```csharp
public readonly record struct SearchQueryResult(
    TrackSearchMode SearchMode,  // The Lavalink4NET search mode
    SearchSource Source,         // The identified platform (enum)
    string Query,                // Clean query without prefix
    string OriginalQuery,        // Original input string
    string? DetectedPrefix,      // The prefix that was found (or null)
    bool IsUrl,                  // True if input was a URL
    bool HasPrefix               // True if a known prefix was detected
)
{
    // Convenience properties
    bool IsJellyfin { get; }     // True if Source == SearchSource.Jellyfin
    bool IsYouTube { get; }      // True if YouTube or YouTube Music
    string SourceName { get; }   // Human-readable name ("Jellyfin", "YouTube", etc.)
}
```

---

### SearchSource Enum

Identifies the search platform/source.

```csharp
public enum SearchSource
{
    None,          // Direct URL or unknown source
    YouTube,       // YouTube (ytsearch:)
    YouTubeMusic,  // YouTube Music (ytmsearch:)
    SoundCloud,    // SoundCloud (scsearch:)
    Spotify,       // Spotify (spsearch:)
    AppleMusic,    // Apple Music (amsearch:)
    Deezer,        // Deezer (dzsearch:)
    YandexMusic,   // Yandex Music (ymsearch:)
    Jellyfin,      // Jellyfin (jfsearch:)
    Custom,        // User-registered custom source
}
```

---

## Examples

### Basic Usage

```csharp
using Lavalink4NET.Jellyfin;
using Lavalink4NET.Rest.Entities.Tracks;

// User searches with a prefix
var userInput = "jfsearch:Bohemian Rhapsody";

// Parse the query - extracts the prefix and cleans the query
var (searchMode, cleanQuery) = SearchQueryParser.Parse(userInput);

// searchMode = JellyfinSearchMode.Jellyfin
// cleanQuery = "Bohemian Rhapsody"

// Load tracks using the detected search mode
var options = new TrackLoadOptions { SearchMode = searchMode };
var result = await audioService.Tracks.LoadTracksAsync(cleanQuery, options);
```

---

### Setting Default Search Provider

Set which provider to use when the user doesn't specify a prefix:

```csharp
// Default to Jellyfin when no prefix is provided
var (searchMode, query) = SearchQueryParser.Parse(
    userInput, 
    defaultMode: JellyfinSearchMode.Jellyfin
);

// Default to YouTube when no prefix is provided
var (searchMode, query) = SearchQueryParser.Parse(
    userInput, 
    defaultMode: TrackSearchMode.YouTube
);

// Default to SoundCloud when no prefix is provided
var (searchMode, query) = SearchQueryParser.Parse(
    userInput, 
    defaultMode: TrackSearchMode.SoundCloud
);
```

**Example behavior with Jellyfin as default:**

| User Input | Search Mode | Query |
|------------|-------------|-------|
| `Bohemian Rhapsody` | Jellyfin | `Bohemian Rhapsody` |
| `jfsearch:Queen` | Jellyfin | `Queen` |
| `ytsearch:Never Gonna Give You Up` | YouTube | `Never Gonna Give You Up` |
| `https://youtube.com/watch?v=...` | None (URL) | `https://youtube.com/watch?v=...` |

---

### Using Extended Parse Results

Get detailed information about the parsed query:

```csharp
// User input could be anything - with or without prefix
string userInput = GetUserInput(); // e.g., "jfsearch:Queen", "ytsearch:Hello", "Bohemian Rhapsody"

var result = SearchQueryParser.ParseExtended(userInput, JellyfinSearchMode.Jellyfin);

// Access detailed information
Console.WriteLine($"Source: {result.Source}");           // e.g., SearchSource.Jellyfin
Console.WriteLine($"Source Name: {result.SourceName}"); // e.g., "Jellyfin"
Console.WriteLine($"Query: {result.Query}");            // e.g., "Queen" (prefix stripped)
Console.WriteLine($"Original: {result.OriginalQuery}"); // e.g., "jfsearch:Queen"
Console.WriteLine($"Prefix: {result.DetectedPrefix}");  // e.g., "jfsearch" or null
Console.WriteLine($"Is URL: {result.IsUrl}");           // false
Console.WriteLine($"Has Prefix: {result.HasPrefix}");   // true if prefix was detected

// Use conditional logic to show different messages based on source
if (result.IsJellyfin)
{
    Console.WriteLine("🏠 Searching your personal Jellyfin library...");
}
else if (result.IsYouTube)
{
    Console.WriteLine("▶️ Searching YouTube...");
}
else if (result.IsUrl)
{
    Console.WriteLine("🔗 Loading from URL...");
}
else
{
    Console.WriteLine($"🔍 Searching {result.SourceName}...");
}

// Still supports deconstruction for simple usage
var (searchMode, cleanQuery) = result;
```

**Example outputs:**

| User Input | Source | Message |
|------------|--------|---------|
| `jfsearch:Queen` | Jellyfin | "🏠 Searching your personal Jellyfin library..." |
| `ytsearch:Hello` | YouTube | "▶️ Searching YouTube..." |
| `Bohemian Rhapsody` | Jellyfin (default) | "🏠 Searching your personal Jellyfin library..." |
| `https://youtube.com/...` | None | "🔗 Loading from URL..." |
| `scsearch:Electronic` | SoundCloud | "🔍 Searching SoundCloud..." |

---

### Registering Custom Prefixes

Add your own search prefixes for custom sources:

```csharp
// Create a custom search mode
var navidromeMode = JellyfinSearchMode.Custom("ndsearch");

// Register it with a custom source type
SearchQueryParser.RegisterPrefix("ndsearch", navidromeMode, SearchSource.Custom);

// Now it works with Parse()
var (mode, query) = SearchQueryParser.Parse("ndsearch:My Song");
// mode = navidromeMode
// query = "My Song"

// And with ParseExtended()
var result = SearchQueryParser.ParseExtended("ndsearch:My Song");
// result.Source = SearchSource.Custom
// result.DetectedPrefix = "ndsearch"

// Check if a prefix is registered
bool isRegistered = SearchQueryParser.IsPrefixRegistered("ndsearch"); // true

// Get all registered prefixes
var allPrefixes = SearchQueryParser.GetRegisteredPrefixes();
// ["jfsearch", "ytsearch", "ytmsearch", "scsearch", "spsearch", "amsearch", "dzsearch", "ymsearch", "ndsearch"]

// Unregister a prefix
SearchQueryParser.UnregisterPrefix("ndsearch");
```

---

### Discord Bot Integration

Complete example for a Discord music bot using Discord.NET and Lavalink4NET:

```csharp
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Jellyfin;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;

public class PlayHandler
{
    private readonly IAudioService _audioService;
    private readonly DiscordSocketClient _client;

    public PlayHandler(IAudioService audioService, DiscordSocketClient client)
    {
        _audioService = audioService;
        _client = client;
    }

    public async Task HandlePlayCommand(
        SocketSlashCommand command, 
        string searchQuery, 
        CancellationToken ct = default)
    {
        // Get or create player for the guild
        var player = await GetPlayerAsync(command);
        if (player is null) return;

        // Parse the query to extract search mode and clean query
        // Supports prefixes like jfsearch:, ytsearch:, scsearch:, etc.
        // Default: Jellyfin when no prefix is specified
        var (searchMode, queryToSearch) = SearchQueryParser.Parse(
            searchQuery, 
            JellyfinSearchMode.Jellyfin
        );

        // Create track load options with the detected search mode
        var trackLoadOptions = new TrackLoadOptions
        {
            SearchMode = searchMode,
        };

        // Load tracks
        var trackCollection = await _audioService.Tracks.LoadTracksAsync(
            queryToSearch, 
            trackLoadOptions, 
            cancellationToken: ct
        );

        // Handle the result
        if (trackCollection.Track is null)
        {
            await command.RespondAsync("❌ No tracks found.");
            return;
        }

        // Play the track
        await player.PlayAsync(trackCollection.Track, cancellationToken: ct);
        
        // Send confirmation with source info
        var result = SearchQueryParser.ParseExtended(searchQuery, JellyfinSearchMode.Jellyfin);
        await command.RespondAsync($"🎵 Now playing from **{result.SourceName}**: {trackCollection.Track.Title}");
    }

    private async Task<QueuedLavalinkPlayer?> GetPlayerAsync(SocketSlashCommand command)
    {
        // ... player retrieval logic
    }
}
```

**Usage in Discord:**

| Command | Behavior |
|---------|----------|
| `/play Bohemian Rhapsody` | Searches Jellyfin (default) |
| `/play jfsearch:Queen` | Searches Jellyfin (explicit) |
| `/play ytsearch:Never Gonna Give You Up` | Searches YouTube |
| `/play scsearch:Electronic Mix` | Searches SoundCloud |
| `/play https://youtube.com/watch?v=dQw4w9WgXcQ` | Plays URL directly |

---

## Supported Prefixes

| Prefix | Platform | SearchSource |
|--------|----------|--------------|
| `jfsearch:` | Jellyfin | `SearchSource.Jellyfin` |
| `ytsearch:` | YouTube | `SearchSource.YouTube` |
| `ytmsearch:` | YouTube Music | `SearchSource.YouTubeMusic` |
| `scsearch:` | SoundCloud | `SearchSource.SoundCloud` |
| `spsearch:` | Spotify | `SearchSource.Spotify` |
| `amsearch:` | Apple Music | `SearchSource.AppleMusic` |
| `dzsearch:` | Deezer | `SearchSource.Deezer` |
| `ymsearch:` | Yandex Music | `SearchSource.YandexMusic` |

---

## Requirements

- **.NET 8.0** or later
- **Lavalink4NET 4.x**
- **Lavalink Server** with appropriate plugins:
  - [Jellylink](https://github.com/Myxelium/Jellylink) for Jellyfin support
  - [LavaSrc](https://github.com/topi314/LavaSrc) for Spotify, Apple Music, Deezer support

---

## License

MIT License - see [LICENSE](LICENSE) for details.


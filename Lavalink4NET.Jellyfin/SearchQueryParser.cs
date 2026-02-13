using Lavalink4NET.Rest.Entities.Tracks;

namespace Lavalink4NET.Jellyfin;

/// <summary>
/// Identifies the source/platform for a search query.
/// </summary>
public enum SearchSource
{
    /// <summary>No specific source (direct URL or unknown).</summary>
    None,
    /// <summary>YouTube search.</summary>
    YouTube,
    /// <summary>YouTube Music search.</summary>
    YouTubeMusic,
    /// <summary>SoundCloud search.</summary>
    SoundCloud,
    /// <summary>Spotify search.</summary>
    Spotify,
    /// <summary>Apple Music search.</summary>
    AppleMusic,
    /// <summary>Deezer search.</summary>
    Deezer,
    /// <summary>Yandex Music search.</summary>
    YandexMusic,
    /// <summary>Jellyfin search.</summary>
    Jellyfin,
    /// <summary>Custom/user-defined source.</summary>
    Custom,
}

/// <summary>
/// Provides extension methods and utilities for parsing search queries with custom prefixes.
/// </summary>
public static class SearchQueryParser
{
    private static readonly Dictionary<string, (TrackSearchMode Mode, SearchSource Source)> RegisteredPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["jfsearch"] = (JellyfinSearchMode.Jellyfin, SearchSource.Jellyfin),
        ["ytsearch"] = (TrackSearchMode.YouTube, SearchSource.YouTube),
        ["ytmsearch"] = (TrackSearchMode.YouTubeMusic, SearchSource.YouTubeMusic),
        ["scsearch"] = (TrackSearchMode.SoundCloud, SearchSource.SoundCloud),
        ["spsearch"] = (TrackSearchMode.Spotify, SearchSource.Spotify),
        ["amsearch"] = (TrackSearchMode.AppleMusic, SearchSource.AppleMusic),
        ["dzsearch"] = (TrackSearchMode.Deezer, SearchSource.Deezer),
        ["ymsearch"] = (TrackSearchMode.YandexMusic, SearchSource.YandexMusic),
    };

    /// <summary>
    /// Parses a query string and extracts the search mode and clean query.
    /// If the query contains a known prefix (e.g., "jfsearch:song name"), returns the appropriate
    /// <see cref="TrackSearchMode"/> and the query without the prefix.
    /// </summary>
    /// <param name="query">The original query string (may contain a prefix like "jfsearch:song").</param>
    /// <param name="defaultMode">The default search mode to use if no prefix is found. Defaults to <see cref="TrackSearchMode.YouTube"/>.</param>
    /// <returns>A <see cref="ParsedSearchQuery"/> containing the search mode and clean query string.</returns>
    public static ParsedSearchQuery Parse(string query, TrackSearchMode defaultMode = default)
    {
        var result = ParseExtended(query, defaultMode);
        return new ParsedSearchQuery(result.SearchMode, result.Query);
    }

    /// <summary>
    /// Parses a query string and returns detailed information about the search source.
    /// </summary>
    /// <param name="query">The original query string (may contain a prefix like "jfsearch:song").</param>
    /// <param name="defaultMode">The default search mode to use if no prefix is found. Defaults to <see cref="TrackSearchMode.YouTube"/>.</param>
    /// <returns>A <see cref="SearchQueryResult"/> containing detailed information about the parsed query.</returns>
    /// <example>
    /// <code>
    /// var result = SearchQueryParser.ParseExtended("jfsearch:Bohemian Rhapsody");
    /// // result.Source == SearchSource.Jellyfin
    /// // result.SearchMode == JellyfinSearchMode.Jellyfin
    /// // result.Query == "Bohemian Rhapsody"
    /// // result.OriginalQuery == "jfsearch:Bohemian Rhapsody"
    /// // result.DetectedPrefix == "jfsearch"
    /// // result.IsUrl == false
    /// // result.HasPrefix == true
    /// </code>
    /// </example>
    public static SearchQueryResult ParseExtended(string query, TrackSearchMode defaultMode = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            var fallback = defaultMode.Equals(default) ? TrackSearchMode.YouTube : defaultMode;
            var fallbackSource = GetSourceFromMode(fallback);
            return new SearchQueryResult(
                SearchMode: fallback,
                Source: fallbackSource,
                Query: query ?? string.Empty,
                OriginalQuery: query ?? string.Empty,
                DetectedPrefix: null,
                IsUrl: false,
                HasPrefix: false
            );
        }

        // Check for known prefixes first (before URL check, as prefixes can look like URIs)
        var colonIndex = query.IndexOf(':');
        if (colonIndex > 0)
        {
            var prefix = query[..colonIndex];
            if (RegisteredPrefixes.TryGetValue(prefix, out var registered))
            {
                var cleanQuery = query[(colonIndex + 1)..].TrimStart();
                return new SearchQueryResult(
                    SearchMode: registered.Mode,
                    Source: registered.Source,
                    Query: cleanQuery,
                    OriginalQuery: query,
                    DetectedPrefix: prefix,
                    IsUrl: false,
                    HasPrefix: true
                );
            }
        }

        // Check if it's a URL - only consider http(s) URLs
        if ((query.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
             query.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
            Uri.IsWellFormedUriString(query, UriKind.Absolute))
        {
            return new SearchQueryResult(
                SearchMode: TrackSearchMode.None,
                Source: SearchSource.None,
                Query: query,
                OriginalQuery: query,
                DetectedPrefix: null,
                IsUrl: true,
                HasPrefix: false
            );
        }

        // No known prefix found, use default
        var defaultFallback = defaultMode.Equals(default) ? TrackSearchMode.YouTube : defaultMode;
        var defaultSource = GetSourceFromMode(defaultFallback);
        return new SearchQueryResult(
            SearchMode: defaultFallback,
            Source: defaultSource,
            Query: query,
            OriginalQuery: query,
            DetectedPrefix: null,
            IsUrl: false,
            HasPrefix: false
        );
    }

    private static SearchSource GetSourceFromMode(TrackSearchMode mode)
    {
        foreach (var kvp in RegisteredPrefixes)
        {
            if (kvp.Value.Mode.Equals(mode))
                return kvp.Value.Source;
        }
        return SearchSource.None;
    }

    /// <summary>
    /// Registers a custom search prefix that can be recognized by <see cref="Parse"/> and <see cref="ParseExtended"/>.
    /// </summary>
    /// <param name="prefix">The prefix to register (without the colon, e.g., "mysearch").</param>
    /// <param name="searchMode">The search mode to associate with this prefix.</param>
    /// <param name="source">The source type for this prefix. Defaults to <see cref="SearchSource.Custom"/>.</param>
    public static void RegisterPrefix(string prefix, TrackSearchMode searchMode, SearchSource source = SearchSource.Custom)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        RegisteredPrefixes[prefix] = (searchMode, source);
    }

    /// <summary>
    /// Unregisters a custom search prefix.
    /// </summary>
    /// <param name="prefix">The prefix to unregister.</param>
    /// <returns>True if the prefix was found and removed; otherwise, false.</returns>
    public static bool UnregisterPrefix(string prefix)
    {
        return RegisteredPrefixes.Remove(prefix);
    }

    /// <summary>
    /// Checks if the given prefix is registered as a known search mode.
    /// </summary>
    /// <param name="prefix">The prefix to check (without the colon).</param>
    /// <returns>True if the prefix is registered; otherwise, false.</returns>
    public static bool IsPrefixRegistered(string prefix)
    {
        return RegisteredPrefixes.ContainsKey(prefix);
    }

    /// <summary>
    /// Gets all registered search prefixes.
    /// </summary>
    /// <returns>A read-only collection of registered prefixes.</returns>
    public static IReadOnlyCollection<string> GetRegisteredPrefixes()
    {
        return RegisteredPrefixes.Keys.ToList().AsReadOnly();
    }

    /// <summary>
    /// Tries to get the search mode and source associated with a prefix.
    /// </summary>
    /// <param name="prefix">The prefix to look up.</param>
    /// <param name="searchMode">When this method returns, contains the search mode if found.</param>
    /// <param name="source">When this method returns, contains the source if found.</param>
    /// <returns>True if the prefix was found; otherwise, false.</returns>
    public static bool TryGetSearchMode(string prefix, out TrackSearchMode searchMode, out SearchSource source)
    {
        if (RegisteredPrefixes.TryGetValue(prefix, out var registered))
        {
            searchMode = registered.Mode;
            source = registered.Source;
            return true;
        }
        searchMode = default;
        source = SearchSource.None;
        return false;
    }

    /// <summary>
    /// Tries to get the search mode associated with a prefix.
    /// </summary>
    /// <param name="prefix">The prefix to look up.</param>
    /// <param name="searchMode">When this method returns, contains the search mode if found.</param>
    /// <returns>True if the prefix was found; otherwise, false.</returns>
    public static bool TryGetSearchMode(string prefix, out TrackSearchMode searchMode)
    {
        return TryGetSearchMode(prefix, out searchMode, out _);
    }
}

/// <summary>
/// Represents the detailed result of parsing a search query.
/// </summary>
/// <param name="SearchMode">The Lavalink4NET search mode to use.</param>
/// <param name="Source">The identified search source/platform.</param>
/// <param name="Query">The clean query string without the prefix.</param>
/// <param name="OriginalQuery">The original query string as provided.</param>
/// <param name="DetectedPrefix">The prefix that was detected, or null if none.</param>
/// <param name="IsUrl">Whether the query is a direct URL.</param>
/// <param name="HasPrefix">Whether a known prefix was detected in the query.</param>
public readonly record struct SearchQueryResult(
    TrackSearchMode SearchMode,
    SearchSource Source,
    string Query,
    string OriginalQuery,
    string? DetectedPrefix,
    bool IsUrl,
    bool HasPrefix
)
{
    /// <summary>
    /// Deconstructs to just the search mode and query for simple usage.
    /// </summary>
    public void Deconstruct(out TrackSearchMode searchMode, out string query)
    {
        searchMode = SearchMode;
        query = Query;
    }

    /// <summary>
    /// Gets whether this result represents a Jellyfin search.
    /// </summary>
    public bool IsJellyfin => Source == SearchSource.Jellyfin;

    /// <summary>
    /// Gets whether this result represents a YouTube search.
    /// </summary>
    public bool IsYouTube => Source == SearchSource.YouTube || Source == SearchSource.YouTubeMusic;

    /// <summary>
    /// Gets a human-readable name for the search source.
    /// </summary>
    public string SourceName => Source switch
    {
        SearchSource.Jellyfin => "Jellyfin",
        SearchSource.YouTube => "YouTube",
        SearchSource.YouTubeMusic => "YouTube Music",
        SearchSource.SoundCloud => "SoundCloud",
        SearchSource.Spotify => "Spotify",
        SearchSource.AppleMusic => "Apple Music",
        SearchSource.Deezer => "Deezer",
        SearchSource.YandexMusic => "Yandex Music",
        SearchSource.Custom => "Custom",
        _ => "Direct"
    };
}

/// <summary>
/// Represents the result of parsing a search query, containing the detected search mode and clean query.
/// </summary>
/// <param name="SearchMode">The detected or default search mode.</param>
/// <param name="Query">The clean query string without the prefix.</param>
public readonly record struct ParsedSearchQuery(TrackSearchMode SearchMode, string Query)
{
    /// <summary>
    /// Deconstructs the parsed query into its components.
    /// </summary>
    /// <param name="searchMode">The search mode.</param>
    /// <param name="query">The clean query.</param>
    public void Deconstruct(out TrackSearchMode searchMode, out string query)
    {
        searchMode = SearchMode;
        query = Query;
    }
}


using Lavalink4NET.Rest.Entities.Tracks;

namespace Lavalink4NET.Jellyfin;

/// <summary>
/// Extension methods for <see cref="TrackLoadOptions"/> to simplify working with Jellyfin and custom search modes.
/// </summary>
public static class TrackLoadOptionsExtensions
{
    /// <summary>
    /// Creates <see cref="TrackLoadOptions"/> configured for Jellyfin search.
    /// </summary>
    /// <returns>A new <see cref="TrackLoadOptions"/> instance with Jellyfin search mode.</returns>
    /// <example>
    /// <code>
    /// var options = TrackLoadOptionsExtensions.ForJellyfin();
    /// var tracks = await audioService.Tracks.LoadTracksAsync("Bohemian Rhapsody", options);
    /// </code>
    /// </example>
    public static TrackLoadOptions ForJellyfin()
    {
        return new TrackLoadOptions
        {
            SearchMode = JellyfinSearchMode.Jellyfin
        };
    }

    /// <summary>
    /// Creates <see cref="TrackLoadOptions"/> from a parsed search query.
    /// </summary>
    /// <param name="parsedQuery">The parsed search query containing the search mode.</param>
    /// <returns>A new <see cref="TrackLoadOptions"/> instance.</returns>
    /// <example>
    /// <code>
    /// var parsed = SearchQueryParser.Parse("jfsearch:Bohemian Rhapsody");
    /// var options = parsed.ToTrackLoadOptions();
    /// var tracks = await audioService.Tracks.LoadTracksAsync(parsed.Query, options);
    /// </code>
    /// </example>
    public static TrackLoadOptions ToTrackLoadOptions(this ParsedSearchQuery parsedQuery)
    {
        return new TrackLoadOptions
        {
            SearchMode = parsedQuery.SearchMode
        };
    }

    /// <summary>
    /// Creates <see cref="TrackLoadOptions"/> with a custom search mode.
    /// </summary>
    /// <param name="searchMode">The search mode to use.</param>
    /// <returns>A new <see cref="TrackLoadOptions"/> instance.</returns>
    /// <example>
    /// <code>
    /// var options = JellyfinSearchMode.Jellyfin.ToTrackLoadOptions();
    /// var tracks = await audioService.Tracks.LoadTracksAsync("Bohemian Rhapsody", options);
    /// </code>
    /// </example>
    public static TrackLoadOptions ToTrackLoadOptions(this TrackSearchMode searchMode)
    {
        return new TrackLoadOptions
        {
            SearchMode = searchMode
        };
    }
}


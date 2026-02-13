using Lavalink4NET.Rest.Entities.Tracks;

namespace Lavalink4NET.Jellyfin;

/// <summary>
/// Provides additional <see cref="TrackSearchMode"/> values for Jellyfin and other custom sources.
/// </summary>
public static class JellyfinSearchMode
{
    /// <summary>
    /// Jellyfin search mode using the <c>jfsearch:</c> prefix.
    /// Use this when searching your Jellyfin library via the Jellylink Lavalink plugin.
    /// </summary>
    /// <example>
    /// <code>
    /// var options = new TrackLoadOptions { SearchMode = JellyfinSearchMode.Jellyfin };
    /// var tracks = await audioService.Tracks.LoadTracksAsync("Bohemian Rhapsody", options);
    /// </code>
    /// </example>
    public static TrackSearchMode Jellyfin { get; } = new("jfsearch");

    /// <summary>
    /// Creates a custom search mode with the specified prefix.
    /// </summary>
    /// <param name="prefix">The search prefix without the colon (e.g., "mysearch" for "mysearch:").</param>
    /// <returns>A new <see cref="TrackSearchMode"/> instance configured with the specified prefix.</returns>
    /// <example>
    /// <code>
    /// var mySearchMode = JellyfinSearchMode.Custom("mycustom");
    /// var options = new TrackLoadOptions { SearchMode = mySearchMode };
    /// </code>
    /// </example>
    public static TrackSearchMode Custom(string prefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        return new TrackSearchMode(prefix);
    }
}


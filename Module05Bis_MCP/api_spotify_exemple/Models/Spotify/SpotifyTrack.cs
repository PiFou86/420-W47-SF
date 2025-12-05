using System.Text.Json.Serialization;

namespace api_spotify_exemple.Models.Spotify;

public class SpotifyTrack
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("track_number")]
    public int TrackNumber { get; set; }

    [JsonPropertyName("duration_ms")]
    public int DurationMs { get; set; }

    [JsonPropertyName("explicit")]
    public bool Explicit { get; set; }

    [JsonPropertyName("artists")]
    public List<SpotifyArtistSimple> Artists { get; set; } = new();

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }

    // Helper pour afficher la durée
    public string FormattedDuration => TimeSpan.FromMilliseconds(DurationMs).ToString(@"mm\:ss");
}

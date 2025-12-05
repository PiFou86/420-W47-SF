using System.Text.Json.Serialization;

namespace api_spotify_exemple.Models.Spotify;

public class SpotifySearchResponse
{
    [JsonPropertyName("artists")]
    public SpotifyArtistsPage? Artists { get; set; }
}

public class SpotifyArtistsPage
{
    [JsonPropertyName("items")]
    public List<SpotifyArtist> Items { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }
}

public class SpotifyAlbumsResponse
{
    [JsonPropertyName("items")]
    public List<SpotifyAlbum> Items { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }
}

public class SpotifyTracksResponse
{
    [JsonPropertyName("items")]
    public List<SpotifyTrack> Items { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }
}

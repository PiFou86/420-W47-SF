using System.Text.Json.Serialization;

namespace api_spotify_exemple.Models.Spotify;

public class SpotifyAlbum
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("album_type")]
    public string AlbumType { get; set; } = string.Empty;

    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; } = string.Empty;

    [JsonPropertyName("total_tracks")]
    public int TotalTracks { get; set; }

    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = new();

    [JsonPropertyName("artists")]
    public List<SpotifyArtistSimple> Artists { get; set; } = new();
}

public class SpotifyArtistSimple
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

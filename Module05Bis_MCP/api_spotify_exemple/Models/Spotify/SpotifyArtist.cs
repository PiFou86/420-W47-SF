using System.Text.Json.Serialization;

namespace api_spotify_exemple.Models.Spotify;

public class SpotifyArtist
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("popularity")]
    public int Popularity { get; set; }

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = new();

    [JsonPropertyName("images")]
    public List<SpotifyImage> Images { get; set; } = new();

    [JsonPropertyName("followers")]
    public SpotifyFollowers? Followers { get; set; }
}

public class SpotifyImage
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }
}

public class SpotifyFollowers
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

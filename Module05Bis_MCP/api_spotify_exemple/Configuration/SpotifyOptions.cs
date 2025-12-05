namespace api_spotify_exemple.Configuration;

public class SpotifyOptions
{
    public const string SectionName = "Spotify";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = "https://accounts.spotify.com/api/token";
    public string ApiBaseUrl { get; set; } = "https://api.spotify.com/";
}

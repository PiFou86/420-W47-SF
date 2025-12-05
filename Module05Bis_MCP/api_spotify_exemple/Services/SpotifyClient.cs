using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using api_spotify_exemple.Configuration;
using api_spotify_exemple.Models.Spotify;

namespace api_spotify_exemple.Services;

public class SpotifyClient : ISpotifyClient
{
    private readonly HttpClient _httpClient;
    private readonly SpotifyOptions _options;
    private readonly ILogger<SpotifyClient> _logger;
    
    // Gestion du token (thread-safe pour Singleton)
    private string? _accessToken;
    private DateTime _tokenExpiration;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public SpotifyClient(
        HttpClient httpClient,
        IOptions<SpotifyOptions> options,
        ILogger<SpotifyClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_options.ApiBaseUrl);
    }

    #region Token Management

    /// <summary>
    /// Obtient un token valide (réutilise ou renouvelle automatiquement)
    /// </summary>
    private async Task<string> GetValidTokenAsync()
    {
        // Si le token est encore valide (avec marge de 5 min)
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiration.AddMinutes(-5))
        {
            return _accessToken;
        }

        // Renouveler le token de manière thread-safe
        await _tokenLock.WaitAsync();
        try
        {
            // Double-check après le lock
            if (_accessToken != null && DateTime.UtcNow < _tokenExpiration.AddMinutes(-5))
            {
                return _accessToken;
            }

            _logger.LogInformation("Renouvellement du token Spotify...");
            
            // Créer la requête d'authentification
            var authString = $"{_options.ClientId}:{_options.ClientSecret}";
            var base64Auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(authString));

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<SpotifyToken>(content);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException("Impossible d'obtenir le token Spotify");
            }

            _accessToken = tokenResponse.AccessToken;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            _logger.LogInformation("Token Spotify obtenu avec succès. Expire dans {ExpiresIn} secondes", 
                tokenResponse.ExpiresIn);

            return _accessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    /// <summary>
    /// Effectue une requête GET à l'API Spotify avec gestion automatique du token
    /// </summary>
    private async Task<T?> GetAsync<T>(string endpoint)
    {
        Console.Out.WriteLine("GetAsync called with endpoint: " + endpoint);
        var token = await GetValidTokenAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Erreur API Spotify: {StatusCode} - {Error}", 
                response.StatusCode, errorContent);
            throw new HttpRequestException(
                $"Erreur API Spotify: {response.StatusCode} - {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content);
    }

    #endregion

    #region Public API Methods

    /// <summary>
    /// Recherche un artiste par son nom
    /// </summary>
    public async Task<List<SpotifyArtist>> SearchArtistAsync(string artistName, int limit = 10, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(artistName))
        {
            throw new ArgumentException("Le nom de l'artiste ne peut pas être vide", nameof(artistName));
        }

        if (limit < 1 || limit > 50)
        {
            throw new ArgumentException("Le limit doit être entre 1 et 50", nameof(limit));
        }

        if (offset < 0)
        {
            throw new ArgumentException("L'offset doit être >= 0", nameof(offset));
        }

        _logger.LogInformation("Recherche de l'artiste: {ArtistName} (limit: {Limit}, offset: {Offset})", 
            artistName, limit, offset);

        var encodedQuery = Uri.EscapeDataString(artistName);
        var endpoint = $"/v1/search?q={encodedQuery}&type=artist&limit={limit}&offset={offset}";

        var response = await GetAsync<SpotifySearchResponse>(endpoint);

        if (response?.Artists?.Items == null)
        {
            return new List<SpotifyArtist>();
        }

        _logger.LogInformation("Trouvé {Count} artiste(s) sur {Total} total (offset: {Offset})", 
            response.Artists.Items.Count, response.Artists.Total, offset);

        return response.Artists.Items;
    }

    /// <summary>
    /// Récupère les albums d'un artiste
    /// </summary>
    public async Task<List<SpotifyAlbum>> GetArtistAlbumsAsync(string artistId, int limit = 20, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(artistId))
        {
            throw new ArgumentException("L'ID de l'artiste ne peut pas être vide", nameof(artistId));
        }

        if (limit < 1 || limit > 50)
        {
            throw new ArgumentException("Le limit doit être entre 1 et 50", nameof(limit));
        }

        if (offset < 0)
        {
            throw new ArgumentException("L'offset doit être >= 0", nameof(offset));
        }

        _logger.LogInformation("Récupération des albums pour l'artiste: {ArtistId} (limit: {Limit}, offset: {Offset})", 
            artistId, limit, offset);

        var endpoint = $"/v1/artists/{artistId}/albums?include_groups=album,single&limit={limit}&offset={offset}";

        var response = await GetAsync<SpotifyAlbumsResponse>(endpoint);

        if (response?.Items == null)
        {
            return new List<SpotifyAlbum>();
        }

        _logger.LogInformation("Trouvé {Count} album(s) sur {Total} total (offset: {Offset})", 
            response.Items.Count, response.Total, offset);

        return response.Items;
    }

    /// <summary>
    /// Récupère les titres d'un album
    /// </summary>
    public async Task<List<SpotifyTrack>> GetAlbumTracksAsync(string albumId, int limit = 50, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(albumId))
        {
            throw new ArgumentException("L'ID de l'album ne peut pas être vide", nameof(albumId));
        }

        if (limit < 1 || limit > 50)
        {
            throw new ArgumentException("Le limit doit être entre 1 et 50", nameof(limit));
        }

        if (offset < 0)
        {
            throw new ArgumentException("L'offset doit être >= 0", nameof(offset));
        }

        _logger.LogInformation("Récupération des titres pour l'album: {AlbumId} (limit: {Limit}, offset: {Offset})", 
            albumId, limit, offset);

        var endpoint = $"/v1/albums/{albumId}/tracks?limit={limit}&offset={offset}";

        var response = await GetAsync<SpotifyTracksResponse>(endpoint);

        if (response?.Items == null)
        {
            return new List<SpotifyTrack>();
        }

        _logger.LogInformation("Trouvé {Count} titre(s) sur {Total} total (offset: {Offset})", 
            response.Items.Count, response.Total, offset);

        return response.Items;
    }

    #endregion
}

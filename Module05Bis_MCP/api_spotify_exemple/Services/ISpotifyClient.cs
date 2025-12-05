using api_spotify_exemple.Models.Spotify;

namespace api_spotify_exemple.Services;

public interface ISpotifyClient
{
    /// <summary>
    /// Recherche un artiste par son nom
    /// </summary>
    /// <param name="artistName">Nom de l'artiste à rechercher</param>
    /// <param name="limit">Nombre maximum de résultats par page (max: 50)</param>
    /// <param name="offset">Position de départ pour la pagination (0 = première page)</param>
    Task<List<SpotifyArtist>> SearchArtistAsync(string artistName, int limit = 10, int offset = 0);

    /// <summary>
    /// Récupère les albums d'un artiste
    /// </summary>
    /// <param name="artistId">ID Spotify de l'artiste</param>
    /// <param name="limit">Nombre maximum de résultats par page (max: 50)</param>
    /// <param name="offset">Position de départ pour la pagination (0 = première page)</param>
    Task<List<SpotifyAlbum>> GetArtistAlbumsAsync(string artistId, int limit = 20, int offset = 0);

    /// <summary>
    /// Récupère les titres d'un album
    /// </summary>
    /// <param name="albumId">ID Spotify de l'album</param>
    /// <param name="limit">Nombre maximum de résultats par page (max: 50)</param>
    /// <param name="offset">Position de départ pour la pagination (0 = première page)</param>
    Task<List<SpotifyTrack>> GetAlbumTracksAsync(string albumId, int limit = 50, int offset = 0);
}

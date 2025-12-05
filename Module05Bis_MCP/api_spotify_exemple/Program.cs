//#define USE_SINGLETON

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using api_spotify_exemple.Configuration;
using api_spotify_exemple.Services;
using api_spotify_exemple.Models.Spotify;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SpotifyOptions>(builder.Configuration.GetSection(SpotifyOptions.SectionName));
builder.Services.AddHttpClient();

#if USE_SINGLETON
builder.Services.AddSingleton<ISpotifyClient, SpotifyClient>();

IHost app = builder.Build();

ISpotifyClient spotifyClient = app.Services.GetRequiredService<ISpotifyClient>();
await EffectuerRequetesSpotify(spotifyClient);

#else
builder.Services.AddScoped<ISpotifyClient, SpotifyClient>();

IHost app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    ISpotifyClient spotifyClient = scope.ServiceProvider.GetRequiredService<ISpotifyClient>();
    await EffectuerRequetesSpotify(spotifyClient);
}
#endif

async Task EffectuerRequetesSpotify(ISpotifyClient spotifyClient)
{
    Console.WriteLine("Recherche d'artistes...");
    List<SpotifyArtist> artists = await spotifyClient.SearchArtistAsync("System Of A Down");
    artists.ForEach(artist => Console.WriteLine(artist.Name));
    Console.WriteLine("Recherche des albums");
    List<SpotifyAlbum> albums = await spotifyClient.GetArtistAlbumsAsync(artists[0].Id);
    albums.ForEach(album => Console.WriteLine(album.Name));
    Console.WriteLine("Recherche des titres du premier album");
    List<SpotifyTrack> tracks = await spotifyClient.GetAlbumTracksAsync(albums[0].Id);
    tracks.ForEach(track => Console.WriteLine(track.Name));
}
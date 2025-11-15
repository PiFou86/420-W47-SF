// PFL : Choisir une des deux methodes pour demarrer l'application : avec hosting ou manuellement

//#define UTILISER_HOSTING
#define MANUEL


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<IClasseAvecConfigurationSimple, ClasseAvecConfigurationSimple>();
builder.Services.AddScoped<IClasseAvecConfigurationStructuree, ClasseAvecConfigurationStructuree>();
builder.Services.AddScoped<MonApplication>();

// Pour la configuration structuree
builder.Services.AddOptions<ExempleConfigurationStructuree>()
    .Bind(builder.Configuration.GetSection("ConfigurationStructuree"));

// Avec hosting
#if UTILISER_HOSTING
builder.Services.AddHostedService<MonApplication>();
await builder.Build().RunAsync();
#else
#if MANUEL
// ou manuellement
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MonApplication monApp = scope.ServiceProvider.GetRequiredService<MonApplication>();
// ...
#endif
#endif

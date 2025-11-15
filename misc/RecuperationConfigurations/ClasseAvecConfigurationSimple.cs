using Microsoft.Extensions.Configuration;

public class ClasseAvecConfigurationSimple : IClasseAvecConfigurationSimple
{
    public ClasseAvecConfigurationSimple(IConfiguration configuration)
    {
        string? valeur = configuration["ClefConfigurationSimple"];
        Console.WriteLine($"Valeur de la clef de configuration simple : {valeur}");
    }
}

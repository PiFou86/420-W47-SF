
using Microsoft.Extensions.Options;

public class ClasseAvecConfigurationStructuree : IClasseAvecConfigurationStructuree
{
    public ClasseAvecConfigurationStructuree(IOptions<ExempleConfigurationStructuree> options)
    {
        ExempleConfigurationStructuree config = options.Value;
        Console.WriteLine($"Valeur de la clef de configuration structuree : {config.SousClef1}");
        Console.WriteLine($"Valeur de la sous clef 2 : {config.SousClef2}");
        Console.WriteLine($"Valeur de la sous clef 3 : {config.SousClef3}");
        Console.WriteLine($"Valeur de la sous clef 4 : {config.SousClef4}");
    }
}

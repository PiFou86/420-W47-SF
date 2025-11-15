using Microsoft.Extensions.Hosting;

public class MonApplication : IHostedService
{
    public MonApplication(IClasseAvecConfigurationSimple simpleConfig, IClasseAvecConfigurationStructuree structureConfig)
    {
        // ...
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("MonApplication demarre.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("MonApplication s'est arretee.");
        return Task.CompletedTask;
    }
}

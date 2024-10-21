using Module05_Models;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Module05_SOAP_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Binding binding = new BasicHttpBinding();
            EndpointAddress endpoint = new EndpointAddress(new Uri("http://localhost:5000/EchoService.svc"));
            ChannelFactory<IEchoService> channelFactory = new ChannelFactory<IEchoService>(binding, endpoint);
            IEchoService echoService = channelFactory.CreateChannel();

            string echo = echoService.Echo("Bonjour DSED !");
            Console.Out.WriteLine($"Echo : {echo}");
            try
            {
                decimal interetCalcule = echoService.CalculInteretAnnuel(-1, 0);
                Console.Out.WriteLine($"Interet1 : {interetCalcule}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Interet1 : {ex.Message}");
            }

            int nombreEssais = 0;
            int nombreMaximumEssais = 3;
            bool appelEffectue = false;
            decimal interet = -1m;
            while (!appelEffectue && nombreEssais < nombreMaximumEssais)
            {
                ++nombreEssais;
                try
                {
                    interet = echoService.CalculInteretAnnuel(100, .199m);
                    appelEffectue = true;
                    Console.Out.WriteLine($"Interet2 : {interet}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Interet2 : {ex.Message} - nombreEssais : {nombreEssais}");
                    if (nombreEssais >= nombreMaximumEssais)
                    {
                        throw;
                    }
                }
            }

            Console.In.ReadLine();
        }
    }
}

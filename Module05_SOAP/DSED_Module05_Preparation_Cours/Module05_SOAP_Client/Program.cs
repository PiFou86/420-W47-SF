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
            try
            {
                echoService.CalculInteretAnnuel(-1, 0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
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
                }
                catch (Exception)
                {
                    if (nombreEssais >= nombreMaximumEssais)
                    {
                        throw;
                    }
                }
            }
        }
    }
}

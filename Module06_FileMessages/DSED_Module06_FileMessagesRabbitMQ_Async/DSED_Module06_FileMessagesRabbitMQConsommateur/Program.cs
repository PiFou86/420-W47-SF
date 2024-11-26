using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSED_Module06_FileMessageProducteur
{
    class Program
    {
        private static ManualResetEvent waitHandle = new ManualResetEvent(false);
        static async Task Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            using (IConnection connexion = await factory.CreateConnectionAsync())
            {
                using (IChannel channel = await connexion.CreateChannelAsync())
                {
                    await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false,
                                                    autoDelete: false, arguments: null
                    );

                    AsyncEventingBasicConsumer consommateur = new AsyncEventingBasicConsumer(channel);
                    consommateur.ReceivedAsync += async (model, ea) =>
                    {
                        byte[] donnees = ea.Body.ToArray();
                        string message = Encoding.UTF8.GetString(donnees);
                        Console.Out.WriteLine(message);
                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                    };
                    await channel.BasicConsumeAsync(queue: "hello",
                        autoAck: false,
                        consumer: consommateur
                    );
                    waitHandle.WaitOne();
                }
            }
        }
    }
}

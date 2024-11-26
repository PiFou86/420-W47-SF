using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DSED_Module06_FileMessagesRabbitMQ
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            using (IConnection connexion = await factory.CreateConnectionAsync())
            {
                using (IChannel channel = await connexion.CreateChannelAsync())
                {
                    await channel.QueueDeclareAsync(
                        queue: "hello",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    for (int i = 0; i < 10; i++)
                    {
                        string message = "mon message - " + Guid.NewGuid();
                        byte[] body = Encoding.UTF8.GetBytes(message);

                        await channel.BasicPublishAsync(exchange: "", routingKey: "hello", body: body);
                    }
                }

            }
        }
    }
}

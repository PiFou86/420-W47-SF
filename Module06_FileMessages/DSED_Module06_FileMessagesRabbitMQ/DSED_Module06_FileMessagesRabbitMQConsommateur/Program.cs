using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace DSED_Module06_FileMessageProducteur
{
    class Program
    {
        private static ManualResetEvent waitHandle = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            using (IConnection connexion = factory.CreateConnection())
            {
                using (IModel channel = connexion.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello", durable: false, exclusive: false,
                                            autoDelete: false, arguments: null
                    );

                    EventingBasicConsumer consommateur = new EventingBasicConsumer(channel);
                    consommateur.Received += (model, ea) =>
                    {
                        byte[] donnees = ea.Body.ToArray();
                        string message = Encoding.UTF8.GetString(donnees);
                        Console.Out.WriteLine(message);
                        channel.BasicAck(ea.DeliveryTag, false);
                    };
                    channel.BasicConsume(queue: "hello",
                        autoAck: false,
                        consumer: consommateur
                    );
                    waitHandle.WaitOne();
                }
            }
        }
    }
}

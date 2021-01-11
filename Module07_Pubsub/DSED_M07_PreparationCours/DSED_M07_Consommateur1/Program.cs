using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace DSED_M07_Consommateur1
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] requetesSujets = { "*.orange.*" };
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            using (IConnection connection = factory.CreateConnection())
            {
                using (IModel channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(
                        exchange: "information_animaux",
                        type: "topic",
                        durable: true,
                        autoDelete: false
                    );
                    channel.QueueDeclare(
                        "consommateur1",
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                        );
                    foreach (var requeteSujet in requetesSujets)
                    {
                        channel.QueueBind(queue: "consommateur1",
                                            exchange: "information_animaux",
                                            routingKey: requeteSujet);
                    }

                    EventingBasicConsumer consumateur = new EventingBasicConsumer(channel);
                    consumateur.Received += (model, ea) =>
                    {
                        byte[] body = ea.Body.ToArray();
                        string message = Encoding.UTF8.GetString(body);
                        string sujet = ea.RoutingKey;
                        Console.WriteLine($"Message reçu \"{message}\" avec le sujet : {sujet}");
                    };
                    channel.BasicConsume(queue: "consommateur1",
                                            autoAck: true,
                                            consumer: consumateur);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}

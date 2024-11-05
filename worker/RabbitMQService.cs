using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System.Text;

public class RabbitMQService
{
    private readonly DiplomaProcessor _processor;

    public RabbitMQService(DiplomaProcessor processor)
    {
        _processor = processor;
    }

    public void StartListening()
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: "diplomasQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            await _processor.ProcessMessageAsync(message);
        };

        channel.BasicConsume(queue: "diplomasQueue", autoAck: true, consumer: consumer);

        Console.WriteLine("Listening to RabbitMQ...");
        Console.ReadLine();
    }
}

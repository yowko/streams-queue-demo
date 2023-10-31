// See https://aka.ms/new-console-template for more information
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory
{
    //設定連線 RabbitMQ username
    UserName = "admin",

    //設定 RabbitMQ password
    Password = "pass.123"
};
var amqpTcpEndpoints = new List<AmqpTcpEndpoint>()
{
    new AmqpTcpEndpoint(new Uri($"amqp://localhost:5672/")),
    new AmqpTcpEndpoint(new Uri($"amqp://localhost:5673/")),
    new AmqpTcpEndpoint(new Uri($"amqp://localhost:5674/"))
};

// Create a connection.
using var connection = factory.CreateConnection(amqpTcpEndpoints);

// Create a channel.
using var channel = connection.CreateModel();

PublishStreams(channel);

// 設定 qos：prefetchSize 只能設寫為 0，prefetchCount 不得設為 0，global 需設為 false
channel.BasicQos(0, 1, false);
// Create a consumer.
var consumer = new EventingBasicConsumer(channel);
// Subscribe to the queue.
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    // Process the message.
    Console.WriteLine($"Received message: {message}");
    // Manually acknowledge the message.
    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};

ConsumeStreams(channel, consumer);


void ConsumeStreams(IModel channel, EventingBasicConsumer consumer)
{
    channel.BasicConsume(
        queue: "test-streams",
        autoAck: false,
        consumer: consumer
        , arguments: new Dictionary<string, object>()
        {
            {
                //"x-stream-offset", "first"
                // "x-stream-offset", "last"
                // "x-stream-offset", "next"
                // "x-stream-offset", "1D" //Y, M, D, h, m, s
                //"x-stream-offset", 1698137600L //無法使用
                "x-stream-offset", 100
            }
        }
    );
    
    Console.ReadLine();
}

void PublishStreams(IModel model)
{
    var message = $@"Hello World! @{DateTime.Now}";
    var body = Encoding.UTF8.GetBytes(message);
    var props = model.CreateBasicProperties();
    model.BasicPublish(exchange: "test",
        routingKey: "streams",
        basicProperties: props,
        body: body);

    Console.WriteLine($"[x] Sent {message} @ {DateTimeOffset.Now.ToUnixTimeSeconds()}");
}
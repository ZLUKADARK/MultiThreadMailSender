using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Publisher.Dto;
using RabbitMQ.Client;
using System.Text;

namespace MailSender.Static
{
    public class Consumer : BackgroundService
    {
        private IModel _channel;
        private readonly IMailSenderService _send;
        CancellationToken token;

        public Consumer(IMailSenderService send)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            _send = send;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            token = stoppingToken;
            Consume();
        }

        public void Consume()
        {
            for (int i = 1; i <= 5; i++)
            {
                Thread thread = new(Send);
                thread.Name = $"Thread №{i}";   // устанавливаем имя для каждого потока
                thread.Start();
            }
        }

        private EmailMessage JsonMessageConvert(string jsonMessage)
        {
            return JsonConvert.DeserializeObject<EmailMessage>(jsonMessage);
        }

        private static bool MessageValidator(EmailMessage message)
        {
            if (string.IsNullOrEmpty(message.Receiver.Address))
            {
                Console.WriteLine("Адрес получателя пуст");
                return false;
            }
            else
            {
                return true;
            }
        }

        private async void Send()
        {
            _channel.ExchangeDeclare(
                exchange: "MultiThread",
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            var queue = _channel.QueueDeclare(
                queue: "MultiThread.Queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null).QueueName;

            _channel.QueueBind(queue, "MultiThread", "MultiThread.Queue");
            
            while (true)
            {
                var result = _channel.BasicGet(queue, false);

                if (result != null)
                {
                    var messageJson = Encoding.UTF8.GetString(result.Body.ToArray());
                    var message = JsonMessageConvert(messageJson);

                    try
                    {
                        await _send.SendMessageAsync(message);
                        Console.WriteLine($"{Thread.CurrentThread.Name} | {message}");
                        _channel.BasicAck(result.DeliveryTag, false);
                        Thread.Sleep(5000);
                    }
                    catch
                    {
                        Thread.Sleep(5000);
                        _channel.BasicNack(result.DeliveryTag, false, true);
                    }
                }
                else
                {
                    Console.WriteLine($"Очередь пуста {Thread.CurrentThread.Name}");
                    Thread.Sleep(5000);
                }
            }
        }
    }
}


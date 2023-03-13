using MailSender.Models.ConfigModels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Publisher.Dto;
using RabbitMQ.Client;
using System.Text;

namespace MailSender.Static
{
    public class Consumer : BackgroundService
    {
        private readonly IModel _channel;
        private readonly IMailSenderService _send;
        private readonly RabbitConfigModel _options;
        CancellationToken token;

        public Consumer(IMailSenderService send, IOptions<RabbitConfigModel> options)
        {
            _send = send;
            _options = options.Value;
            var factory = new ConnectionFactory() { 
                HostName = _options.HostName, 
                Password = _options.Password, 
                UserName = _options.Username, 
                Port = _options.Port, 
                VirtualHost = _options.VirtualHost 
            };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();  
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
                exchange: _options.QueueConfig.QueueName,
                type: _options.ExchangeConfig.Type,
                durable: _options.ExchangeConfig.Durable,
                autoDelete: _options.ExchangeConfig.AutoDelete);

            var queue = _channel.QueueDeclare(
                queue: _options.QueueConfig.QueueName,
                durable: _options.QueueConfig.Durable,
                exclusive: _options.QueueConfig.Exclusive,
                autoDelete: _options.QueueConfig.AutoDelete,
                arguments: null).QueueName;

            _channel.QueueBind(queue, _options.ExchangeConfig.ExchangeName, _options.QueueConfig.RoutingKey);
            
            while (!token.IsCancellationRequested)
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
                        //Thread.Sleep(5000);
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


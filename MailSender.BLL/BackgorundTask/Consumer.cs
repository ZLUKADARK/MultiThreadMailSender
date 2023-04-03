using MailSender.Models.ConfigModels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly MailSenderConfig _senderOptions;
        private readonly ILogger<Consumer> _logger;
        CancellationToken token;

        public Consumer(IMailSenderService send, 
            IOptions<RabbitConfigModel> options, 
            IOptions<MailSenderConfig> senderOptions,
            ILogger<Consumer> logger)
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
            _senderOptions = senderOptions.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            token = stoppingToken;
            Consume();
        }

        public void Consume()
        {
            for (int i = 1; i <= _senderOptions.Threads; i++)
            {
                Thread thread = new(Send);
                thread.Name = $"Thread №{i}";   // устанавливаем имя для каждого потока
                thread.Start();
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
                    if (message != null)
                    {
                        try
                        {
                            await _send.SendMessageAsync(message);
                            _channel.BasicAck(result.DeliveryTag, false);
                            Thread.Sleep(_senderOptions.WaitMilliseconds);
                        }
                        catch
                        {
                            _channel.BasicNack(result.DeliveryTag, false, true);
                        }
                    }
                    _channel.BasicAck(result.DeliveryTag, false);
                }
            }
        }

        private EmailMessage JsonMessageConvert(string jsonMessage)
        {
            var message = JsonConvert.DeserializeObject<EmailMessage>(jsonMessage);
            try
            {
                if (string.IsNullOrEmpty(message.ContentPlain) && string.IsNullOrEmpty(message.ContentHtml))
                {
                    throw new Exception("Message body empty");
                }
                if (string.IsNullOrEmpty(message.Receiver.Address))
                {
                    throw new Exception("Message receiver address empty");
                }
                if (string.IsNullOrEmpty(message.Subject))
                {
                    _logger.LogWarning($"Warning | {DateTime.Now} | Email Message Convert Warning" +
                        $" | WM: Message subject empty | To: {message.Receiver.Address} From: {message.SenderName}");
                }
                return message;
            }
            catch(Exception ex) 
            {
                _logger.LogError($"Error | {DateTime.Now} | Email Message Convert Problem" +
                    $" | EM: {ex.Message} | To: {message.Receiver.Address} From: {message.SenderName}");
                return null;
            }
        }
    }
}


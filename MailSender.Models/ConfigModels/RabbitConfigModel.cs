namespace MailSender.Models.ConfigModels
{
    public class RabbitConfigModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public string VirtualHost { get; set; }
        public int Port { get; set; }
        public QueueConfig QueueConfig { get; set; }
        public ExchangeConfig ExchangeConfig { get; set; }
    }
}

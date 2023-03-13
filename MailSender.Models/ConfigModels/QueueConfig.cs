namespace MailSender.Models.ConfigModels
{
    public class QueueConfig
    {
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }
    }
}

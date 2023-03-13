namespace MailSender.Models.ConfigModels
{
    public class ExchangeConfig
    {
        public string ExchangeName { get; set; }
        public string Type { get; set; }
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }
    }
}

namespace MailSender.Models.ConfigModels
{
    public class MailConfigModel
    {
        public string FromEmail { get; set; }
        public string Password { get; set; }
        public string SMTP { get; set; }
        public int Port { get; set; }
        public bool SSL { get; set; }
    }
}

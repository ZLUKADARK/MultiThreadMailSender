namespace MailSender.Models.ConfigModels
{
    public class MailSenderConfig
    {
        public int WaitMilliseconds { get; set; } = 0;
        public int Threads { get; set; } = 1;
    }
}

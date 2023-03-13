using MailSender.Models.ConfigModels;
using Microsoft.Extensions.Options;
using Publisher.Dto;
using System.Net;
using System.Net.Mail;

public class MailSenderService : IMailSenderService 
{
    private MailConfigModel _mailConfig;

    public MailSenderService(IOptions<MailConfigModel> options)
    {
        _mailConfig = options.Value;
    }

    public async Task SendMessageAsync(EmailMessage message)
    {
        MailAddress from = new MailAddress(_mailConfig.FromEmail, message.SenderName);
        MailAddress to = new MailAddress(message.Receiver.Address, message.Receiver.DisplayName);
        MailMessage mailMessage = new MailMessage(from, to);
        mailMessage.Subject = message.Subject;

        if (message.ContentHtml != null)
        {
            mailMessage.Body = message.ContentHtml;
            mailMessage.IsBodyHtml = true;
        }
        else
        {
            mailMessage.Body = message.ContentPlain;
            mailMessage.IsBodyHtml = false;
        }

        await SendAsync(mailMessage);        
    }

    private async Task SendAsync(MailMessage message)
    {
        try
        {
            SmtpClient smtpClient = new SmtpClient(_mailConfig.SMTP, _mailConfig.Port);
            smtpClient.Credentials = new NetworkCredential(_mailConfig.FromEmail, _mailConfig.Password);
            smtpClient.EnableSsl = _mailConfig.SSL;
            await smtpClient.SendMailAsync(message);
            Console.WriteLine($"Доставлено | {Thread.CurrentThread.ManagedThreadId} | {message.To}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка отправки | Поток Id: {Thread.CurrentThread.ManagedThreadId} | {message.To} {message.Body}");
            Console.WriteLine($"Ошибка | Поток Id: {Thread.CurrentThread.ManagedThreadId} | {ex.Message}");
            throw new Exception();
        }
    }
}

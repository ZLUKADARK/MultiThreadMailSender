using MailSender.Models.ConfigModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Publisher.Dto;
using System.Net;
using System.Net.Mail;

public class MailSenderService : IMailSenderService 
{
    private readonly MailConfigModel _mailConfig;
    private readonly ILogger<MailSenderService> _logger;

    public MailSenderService(IOptions<MailConfigModel> options, ILogger<MailSenderService> logger)
    {
        _mailConfig = options.Value;
        _logger = logger;
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
            smtpClient.Dispose();
            _logger.LogInformation($"Succes | {DateTime.Now} | To: {message.To} From: {message.From.DisplayName}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error | {DateTime.Now} | EM: {ex.Message} | To: {message.To} From: {message.From.DisplayName}");
            throw ex;
        }
    }
}

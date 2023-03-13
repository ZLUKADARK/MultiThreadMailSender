using Publisher.Dto;

public interface IMailSenderService
{
	public Task SendMessageAsync(EmailMessage message);
}

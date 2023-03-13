namespace Publisher.Dto;
public class EmailMessage
{
    public string Id { get; set; }

    public string Subject { get; set; }

    public string SenderName { get; set; }

    public EmailReceiverInformation Receiver { get; set; }

    public string ContentPlain { get; set; }

    public string ContentHtml { get; set; }
}
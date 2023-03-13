namespace Publisher.Dto;
public class EmailReceiverInformation
{
    public EmailReceiverInformation(string displayName, string address)
    {
        DisplayName = displayName;
        Address = address;
    }

    public string DisplayName { get; set; }

    public string Address { get; set; }
}

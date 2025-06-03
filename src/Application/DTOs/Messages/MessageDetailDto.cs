namespace Sdi_Api.Application.DTOs.Messages;

public class MessageDetailDto : MessageDto
{
    public string FullBody { get; set; } = string.Empty;
    public List<MessageDto> PreviousMessagesInThread { get; set; } = new List<MessageDto>(); // Optional
}

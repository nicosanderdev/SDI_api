namespace Sdi_Api.Application.DTOs.Messages;

public class TabCountsDto
{
    public int Inbox { get; set; }
    public int Starred { get; set; }
    public int Replied { get; set; } // Messages current user has replied to
    public int Archived { get; set; }
    public int Sent { get; set; }
    public int Trash { get; set; }
}

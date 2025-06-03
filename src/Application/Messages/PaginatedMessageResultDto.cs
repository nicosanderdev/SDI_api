using Sdi_Api.Application.DTOs.Messages;

namespace SDI_Api.Application.Messages;

public class PaginatedMessageResultDto
{
    public List<MessageDto> Data { get; set; } = new List<MessageDto>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
}

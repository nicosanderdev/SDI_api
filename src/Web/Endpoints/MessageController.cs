using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.Common.Security;
using Sdi_Api.Application.DTOs.Messages;
using SDI_Api.Application.Messages;
using SDI_Api.Domain.Entities;
using SendMessageDto = SDI_Api.Application.Messages.SendMessageDto;

namespace SDI_Api.Web.Endpoints;

[Authorize]
[Route("api/messages")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly ISender _sender;

    public MessagesController(ISender sender)
    {
        _sender = sender;
    }

    // GET: api/messages
    [HttpGet]
    public async Task<ActionResult<PaginatedMessageResultDto>> GetMessages([FromQuery] GetMessagesQuery query)
    {
        try
        {
            var result = await _sender.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message });}
        catch (Exception ex) { return BadRequest(new { message = "Error fetching messages.", error = ex.Message }); }
    }

    // GET: api/messages/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<MessageDetailDto>> GetMessageById(string id)
    {
        if (!Guid.TryParse(id, out var guidId)) return BadRequest("Invalid message ID format.");
        // try/catch for _sender.Send(new GetMessageByIdQuery(guidId));
        // ...
         return Ok(await _sender.Send(new GetMessageByIdQuery(guidId))); // Assuming GetMessageByIdQuery exists
    }

    // POST: api/messages
    [HttpPost]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] SendMessageDto messageData)
    {
        try
        {
            var command = new SendMessageCommand { MessageData = messageData };
            var result = await _sender.Send(command);
            // Return 201 Created with location header and the created message
            return CreatedAtAction(nameof(GetMessageById), new { id = result.Id }, result);
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message });}
        catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (BadRequestException ex) { return BadRequest(new { message = ex.Message }); }
        catch (Exception ex) { return BadRequest(new { message = "Error sending message.", error = ex.Message }); }
    }

    // --- Status Update Endpoints (Example: MarkAsRead) ---
    // PATCH: api/messages/{id}/read
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkMessageAsRead(string id)
    {
        if (!Guid.TryParse(id, out var guidId)) return BadRequest("Invalid message ID format.");
        // await _sender.Send(new MarkMessageAsReadCommand(guidId));
        // ...
        return NoContent();
    }

    // PATCH: api/messages/{id}/unread
    // ... similar structure for unread, star, unstar, archive, unarchive

    // DELETE: api/messages/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(string id)
    {
        if (!Guid.TryParse(id, out var guidId)) return BadRequest("Invalid message ID format.");
        // await _sender.Send(new DeleteMessageCommand(guidId));
        // ...
        return NoContent();
    }
    
    // GET: api/messages/counts
    [HttpGet("counts")]
    public async Task<ActionResult<TabCountsDto>> GetMessageCounts()
    {
         // return Ok(await _sender.Send(new GetMessageCountsQuery())); // Assuming GetMessageCountsQuery exists
         await Task.CompletedTask; // Placeholder
         return Ok(new TabCountsDto()); // Placeholder
    }

    // Helper method for other status updates
    private async Task<IActionResult> UpdateMessageRecipientStatus(Guid messageId, Func<MessageRecipient, bool> updateAction, string successMessage = "Status updated.", string notFoundMessage = "Message not found for user.")
    {
        var currentUserId = /* Get from ICurrentUserService */ Guid.NewGuid(); // Placeholder
         var messageRecipient = await _context.MessageRecipients
            .FirstOrDefaultAsync(mr => mr.MessageId == messageId && mr.RecipientId == currentUserId);

        if (messageRecipient == null) return NotFound(new { message = notFoundMessage });

        bool changed = updateAction(messageRecipient);
        if(changed) await _context.SaveChangesAsync();
        
        return NoContent(); // Or Ok(new { message = successMessage });
    }
}

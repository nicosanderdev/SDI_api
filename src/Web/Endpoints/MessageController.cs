using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.Common.Exceptions;
using SDI_Api.Application.Common.Security;
using Sdi_Api.Application.DTOs.Messages;
using SDI_Api.Application.DTOs.Messages;
using SDI_Api.Application.Messages;
using SDI_Api.Application.Messages.Commands;
using SDI_Api.Application.Messages.Queries;
using SDI_Api.Domain.Exceptions;

namespace SDI_Api.Web.Endpoints;

// [Authorize]
[Route("api/messages")]
[ApiController]
public class MessagesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMessages([FromQuery] GetMessagesQuery query)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdValue, out Guid userId);
        query.UserId = userId;
        var result = await sender.Send(query);
        return Ok(result);
    }
    
    [HttpGet("{messageId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMessageById([FromRoute] string messageId)
    {
        Guid.TryParse(messageId, out var messageGuidId);
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdValue, out Guid userId);
        return Ok(await sender.Send(new GetMessageByIdQuery(messageGuidId, userId)));
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto messageData)
    {   
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdValue, out Guid userId);
        var command = new SendMessageCommand() { MessageData = messageData };
        command.UserId = userId;
        var result = await sender.Send(command);
        return Created("api/messages", result);
    }
    
    [HttpGet("{id}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkMessageAsRead([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid message ID format.");
        
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new ForbiddenAccessException();
        
        await sender.Send(new MarkMessageAsReadCommand(guidId, userId));
        return Ok();
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMessage(string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid message ID format.");
        
        await sender.Send(new DeleteMessageCommand(guidId));
        return Ok();
    }
    
    [HttpGet("counts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TabCountsDto>> GetMessageCounts()
    {
         // return Ok(await _sender.Send(new GetMessageCountsQuery())); // Assuming GetMessageCountsQuery exists
         await Task.CompletedTask; // Placeholder
         return Ok(new TabCountsDto()); // Placeholder
    }
    
    // private async Task<IActionResult> UpdateMessageRecipientStatus(Guid messageId, Func<MessageRecipient, bool> updateAction, string successMessage = "Status updated.", string notFoundMessage = "Message not found for user.")
    //     if (!Guid.TryParse(id, out var guidId)) 
    //         return BadRequest("Invalid message ID format.");
    //         await _sender.Send(new UpdateMessageRecipientStatusCommand(guidId));
    //         return NoContent();
    //
    //
    //     // TODO - NS revisar
    //     var currentUserId = /* Get from ICurrentUserService */ Guid.NewGuid(); // Placeholder
    //      var messageRecipient = await _context.MessageRecipients
    //         .FirstOrDefaultAsync(mr => mr.MessageId == messageId && mr.RecipientId == currentUserId);
    //
    //     if (messageRecipient == null) return NotFound(new { message = notFoundMessage });
    //
    //     bool changed = updateAction(messageRecipient);
    //     if(changed) await _context.SaveChangesAsync();
    //     
    //     return NoContent(); // Or Ok(new { message = successMessage });
    // }
}

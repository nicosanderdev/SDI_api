﻿using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.Common.Security;
using Sdi_Api.Application.DTOs.Messages;
using SDI_Api.Application.Messages;
using SDI_Api.Application.Messages.Commands;
using SDI_Api.Application.Messages.Queries;
using SDI_Api.Domain.Exceptions;
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
    
    [HttpGet]
    public async Task<ActionResult<PaginatedMessageResultDto>> GetMessages([FromQuery] GetMessagesQuery query)
    {
        var result = await _sender.Send(query);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<MessageDetailDto>> GetMessageById(string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid message ID format.");
        
        return Ok(await _sender.Send(new GetMessageByIdQuery(guidId))); // Assuming GetMessageByIdQuery exists
    }
    
    [HttpPost]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] SendMessageDto messageData)
    {
        var command = new SendMessageCommand() { MessageData = messageData };
        var result = await _sender.Send(command);
        return CreatedAtAction(nameof(GetMessageById), new { id = result.Id }, result);
    }

    // --- Status Update Endpoints ---
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkMessageAsRead(string id)
    {
        if (!Guid.TryParse(id, out var guidId)) 
            throw new ArgumentException("Invalid message ID format.");
        
        await _sender.Send(new MarkMessageAsReadCommand(guidId));
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(string id)
    {
        if (!Guid.TryParse(id, out var guidId)) 
            throw new ArgumentException("Invalid message ID format.");
        
        await _sender.Send(new DeleteMessageCommand() { Id = guidId });
        return NoContent();
    }
    
    [HttpGet("counts")]
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

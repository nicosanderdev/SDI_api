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
using ForbiddenAccessException = SDI_Api.Application.Common.Exceptions.ForbiddenAccessException;

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
    
    [HttpGet("property/{propertyId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMessagesByPropertyId([FromRoute] string propertyId)
    {
        if (!Guid.TryParse(propertyId, out var propertyGuidId))
            throw new ArgumentException("Invalid property ID format.");
        
        var query = new GetMessagesByPropertyIdQuery(propertyGuidId);
        var result = await sender.Send(query);
        return Ok(result);
    }
    
    [HttpGet("thread/{threadId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessagesByThreadId(
        [FromRoute] string threadId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 100,
        [FromQuery] string? sortBy = "createdAt_asc")
    {
        if (!Guid.TryParse(threadId, out var threadGuid))
            throw new ArgumentException("Invalid thread ID format.");

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdValue, out Guid userId);

        var query = new GetMessagesByThreadIdQuery(threadGuid)
        {
            UserId = userId,
            Page = page,
            Limit = limit,
            SortBy = sortBy
        };

        var result = await sender.Send(query);
        return Ok(result);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto messageData)
    {   
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdValue, out Guid userId);
        if (userId == Guid.Empty)
            throw new ForbiddenAccessException();
            
        var command = new SendMessageCommand()
        {
            MessageData = messageData,
            UserId = userId
        };
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

    [HttpPatch("{id}/unread")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkMessageAsUnread([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid message ID format.");

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new ForbiddenAccessException();

        await sender.Send(new MarkMessageAsUnreadCommand(guidId, userId));
        return Ok();
    }

    [HttpPatch("{id}/replied")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkMessageAsReplied([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid message ID format.");

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new ForbiddenAccessException();

        await sender.Send(new MarkMessageAsRepliedCommand(guidId, userId));
        return Ok();
    }

    [HttpPatch("{id}/star")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StarMessage([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid message ID format.");

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new ForbiddenAccessException();

        await sender.Send(new StarMessageCommand(guidId, userId));
        return Ok();
    }

    [HttpDelete("{id}/unstar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnstarMessage([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid message ID format.");

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new ForbiddenAccessException();

        await sender.Send(new UnstarMessageCommand(guidId, userId));
        return Ok();
    }

    [HttpPost("{id}/archive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ArchiveMessage([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid message ID format.");

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new ForbiddenAccessException();

        await sender.Send(new ArchiveMessageCommand(guidId, userId));
        return Ok();
    }

    [HttpPatch("{id}/unarchive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnarchiveMessage([FromRoute] string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid message ID format.");

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new ForbiddenAccessException();

        await sender.Send(new UnarchiveMessageCommand(guidId, userId));
        return Ok();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteMessage(string id)
    {
        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException("Invalid message ID format.");

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new ForbiddenAccessException();

        await sender.Send(new SoftDeleteMessageCommand(guidId, userId));
        return Ok();
    }
    
    [HttpGet("counts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TabCountsDto>> GetMessageCounts()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            throw new ForbiddenAccessException();
        
         var result = await sender.Send(new GetMessageCountsQuery { UserId = userId });
         return Ok(result);
    }
}

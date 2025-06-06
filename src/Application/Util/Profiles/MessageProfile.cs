using Sdi_Api.Application.DTOs.Messages;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util.Profiles;

public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            // For MessageDto and MessageDetailDto
            // This mapping assumes you have the Message, MessageRecipient (for current user), Thread, Sender, Property loaded.
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ThreadId, opt => opt.MapFrom(src => src.ThreadId.ToString()))
                .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId.ToString()))
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => $"{src.Sender.FirstName} {src.Sender.LastName}".Trim()))
                .ForMember(dest => dest.PropertyId, opt => opt.MapFrom(src => src.Thread.PropertyId.HasValue ? src.Thread.PropertyId.Value.ToString() : null))
                .ForMember(dest => dest.PropertyTitle, opt => opt.MapFrom(src => src.Thread.Property != null ? src.Thread.Property.Title : null))
                .ForMember(dest => dest.Subject, opt => opt.MapFrom(src => src.Thread.Subject))
                .ForMember(dest => dest.Snippet, opt => opt.MapFrom(src => src.Snippet))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAtUtc.ToString("o"))) // ISO 8601
                // User-specific flags from MessageRecipient. These need to be mapped in the query handler after fetching the specific MessageRecipient.
                // AutoMapper can't easily pick these up from a collection based on current user ID without context.
                // So, these will be set manually in the query handler or with a custom resolver needing ICurrentUserService.
                // For simplicity, the query handler will project these.
                .ForMember(dest => dest.IsRead, opt => opt.Ignore())
                .ForMember(dest => dest.IsReplied, opt => opt.Ignore())
                .ForMember(dest => dest.IsStarred, opt => opt.Ignore())
                .ForMember(dest => dest.IsArchived, opt => opt.Ignore())
                .ForMember(dest => dest.RecipientId, opt => opt.Ignore());


            CreateMap<Message, MessageDetailDto>()
                .IncludeBase<Message, MessageDto>() // Inherit common mappings
                .ForMember(dest => dest.FullBody, opt => opt.MapFrom(src => src.Body));
                // Similar to MessageDto, user-specific flags are ignored here for handler projection.

            // No direct mapping for SendMessageDto to Message entity as it involves complex logic (thread creation, etc.)
        }
    }

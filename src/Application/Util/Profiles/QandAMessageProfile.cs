using SDI_Api.Application.EstateProperties.Commands;
using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util.Profiles;

public class QandAMessageProfile : Profile
{
    public QandAMessageProfile()
    {
        CreateMap<SubmitQandAMessageRequest, QandAMessage>();
    }
}

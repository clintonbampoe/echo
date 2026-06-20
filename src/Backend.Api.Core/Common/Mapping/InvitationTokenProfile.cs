using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class InvitationTokenProfile : Profile
{
    public InvitationTokenProfile()
    {
        CreateMap<InvitationTokenCreateDto, InvitationToken>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Congregation, opt => opt.Ignore())
            .ForMember(dest => dest.CongregationId, opt => opt.Ignore());
    }
}

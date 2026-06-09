using AutoMapper;
using Backend.Api.Core.Dtos;
using Backend.Api.Core.Entities;

namespace Backend.Api.Core.Common.Mapping;

public class TransactionCategoryProfile : Profile
{
    public TransactionCategoryProfile()
    {
        CreateMap<TransactionCategory, TransactionCategoryResponseDto>();
        CreateMap<TransactionCategory, TransactionCategoryListResponseDto>();

        CreateMap<TransactionCategoryCreateDto, TransactionCategory>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<TransactionCategoryUpdateDto, TransactionCategory>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

        CreateMap<TransactionCategoryDeleteDto, TransactionCategory>()
            .ForAllMembers(opt => opt.Ignore());
    }
}

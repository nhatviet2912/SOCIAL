using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using Domain.Enum;

namespace Application.DTO.Response.User;

public class UserResponse : IMapFrom<ApplicationUser>
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public Guid MemberId { get; set; }
    public string? Name { get; set; }
    public bool EmailConfirmed { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ApplicationUser, UserResponse>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Member.Name))
            .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed));
    }
}
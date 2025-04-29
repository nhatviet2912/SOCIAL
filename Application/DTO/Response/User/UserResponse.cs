using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.DTO.Response.User;

public class UserResponse : IMapFrom<ApplicationUser>
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public Guid MemberId { get; set; }
    public string? Name { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<ApplicationUser, UserResponse>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Member.Name));
    }
}
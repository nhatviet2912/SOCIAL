using Application.Common.Mappings;
using AutoMapper;

namespace Application.DTO.Response.RefreshToken;

public class RefreshTokenResponse : IMapFrom<Domain.Entities.RefreshToken>
{
    public string Token { get; set; }
    public string DeviceInfo { get; set; }
    public string RemoteIpAddress { get; set; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Domain.Entities.RefreshToken, RefreshTokenResponse>();
    }
}
using Application.Common.Model;
using Application.DTO.Request.Login;
using Application.DTO.Response.User;

namespace Application.Common.Interfaces.Service;

public interface IExternalLoginService
{
    Task<Result<TokenResponse>> VerifyGoogleToken(GoogleTokenRequest request);
}
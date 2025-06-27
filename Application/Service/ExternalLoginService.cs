using Application.Common.Exception;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Service;
using Application.Common.Model;
using Application.DTO.Request.Login;
using Application.DTO.Response.User;
using Domain.Constants;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Application.Service;

public class ExternalLoginService : IExternalLoginService
{
    private readonly IConfiguration _configuration;
    private readonly IJWTHandler _jwtHandler;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDateTimeService _dateTimeService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public ExternalLoginService(IConfiguration configuration, 
        IJWTHandler jwtHandler,  
        UserManager<ApplicationUser> userManager,
        IDateTimeService dateTimeService,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _jwtHandler = jwtHandler;
        _userManager = userManager;
        _dateTimeService = dateTimeService;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }
    public async Task<Result<TokenResponse>> VerifyGoogleToken(GoogleTokenRequest request)
    {
        var now = _dateTimeService.GetCurrentDateTimeAsync();
        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new List<string>() { _configuration["Authentication:Google:ClientId"]! }
        };
        
        var payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, settings);
        
        if (payload == null)
            throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.INVALID_EXTERNAL);

        var info = new UserLoginInfo(CommonMessage.GoogleProvider, payload.Subject, CommonMessage.GoogleProvider);
        
        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = payload.Name,
                        Email = payload.Email,
                        EmailConfirmed = true,
                        Member = new Member()
                        {
                            Id = Guid.NewGuid(),
                            Name = payload.Name
                        }
                    };
                    await _unitOfWork.UserManager.CreateAsync(user);
                    await _unitOfWork.UserManager.AddToRoleAsync(user, Role.User);
                }
                // Add login if not exists
                var userLogins = await _userManager.GetLoginsAsync(user);
                if (!userLogins.Any(x => x.LoginProvider == info.LoginProvider && x.ProviderKey == info.ProviderKey))
                {
                    await _unitOfWork.UserManager.AddLoginAsync(user, info);
                }
            }
        
            if (user == null)
                throw new CustomException(StatusCodes.Status400BadRequest, ErrorMessageResponse.INVALID_EXTERNAL);
        
            var token = await _jwtHandler.GenerateJwtAsync(user);
        
            var refresh = new RefreshToken
            {
                UserId = user.Id,
                Token = await _jwtHandler.GenerateRefreshToken(),
                Expires = now.AddDays(7),
                CreatedByIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                DeviceInfo = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString(),
                RemoteIpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
            };
            await _unitOfWork.Repository<RefreshToken>().AddAsync(refresh);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            var tokenResponse = new TokenResponse(token, refresh.Token, refresh.Expires);
            return await Result<TokenResponse>.SuccessAsync(tokenResponse, ResponseCode.SUCCESS, StatusCodes.Status200OK);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return await Result<TokenResponse>.FailureAsync(null, ResponseCode.INTERNAL_ERROR, StatusCodes.Status500InternalServerError);
        }
    }
}
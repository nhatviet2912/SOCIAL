using Application.Common.Interfaces.Service;
using Application.DTO.Request.Login;
using Application.DTO.Request.Register;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controller;

public class AuthController : BaseController
{
    private readonly ILogger<AuthController> _logger;
    private readonly IIdentityService _identityService;
    public AuthController(IIdentityService identityService, ILogger<AuthController> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }
    
    [HttpPost("Register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _identityService.CreateUserAsync(request);
        return Ok(result);
    }
    
    [HttpGet("GetAllUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _identityService.GetAllUsersAsync();
        return Ok(result);
    }
    
    [HttpPost("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _identityService.LoginAsync(request);
        return Ok(result);
    }
}
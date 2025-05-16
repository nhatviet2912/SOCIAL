using Application.Common.Interfaces.Service;
using Application.DTO.Request.Login;
using Application.DTO.Request.Register;
using Application.DTO.Request.Role;
using Application.DTO.Request.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controller;

public class AuthController : BaseController
{
    private readonly IIdentityService _identityService;
    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    
    [HttpPost("Register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterAsync(RegisterRequest request)
    {
        var result = await _identityService.CreateUserAsync(request, Request.Headers["origin"]);
        return Ok(result);
    }
    
    [HttpGet("GetAllUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "AdminManager")]
    public async Task<IActionResult> GetAllUsersAsync()
    {
        var result = await _identityService.GetAllUsersAsync();
        return Ok(result);
    }
    
    [HttpPost("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LoginAsync(LoginRequest request)
    {
        var result = await _identityService.LoginAsync(request);
        return Ok(result);
    }
    
    [HttpPost("CreateRole")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "AdminManager")]
    public async Task<IActionResult> CreateRoleAsync(RoleRequest request)
    {
        var result = await _identityService.CreateRoleAsync(request);
        return Ok(result);
    }
    
    [HttpPost("AssignRoles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "AdminManager")]
    public async Task<IActionResult> AssignRolesAsync(AssignRoleRequest request)
    {
        var result = await _identityService.AssignRolesAsync(request);
        return Ok(result);
    }

    [HttpPost("Logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> LogoutAsync(RefreshTokenRequest request)
    {
        var result = await _identityService.LogoutAsync(request);
        return Ok(result);
    }

    [HttpPost("Logout_All")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> LogoutAllAsync()
    {
        var result = await _identityService.LogoutAllAsync();
        return Ok(result);
    }

    [HttpPost("Logout_Devices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> LogoutDevicesAsync()
    {
        var result = await _identityService.LogoutDevicesAsync();
        return Ok(result);
    }

    [HttpGet("GetActiveDevices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> GetActiveDevicesAsync()
    {
        var result = await _identityService.GetActiveDevicesAsync();
        return Ok(result);
    }

    [HttpPost("RefreshToken")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var result = await _identityService.RefreshTokenAsync(request);
        return Ok(result);
    }

    [HttpGet("Confirm-Email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string token)
    {
        var result = await _identityService.ConfirmEmailAsync(userId, token);
        return Ok(result);
    }
}
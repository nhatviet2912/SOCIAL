using Application.Common.Interfaces.Service;
using Application.DTO.Request.Login;
using Application.DTO.Request.Register;
using Application.DTO.Request.Role;
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
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _identityService.CreateUserAsync(request);
        return Ok(result);
    }
    
    [HttpGet("GetAllUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "RequiredAdminManager")]
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
    
    [HttpPost("CreateRole")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "RequiredAdminManager")]
    public async Task<IActionResult> CreateRole(RoleRequest request)
    {
        var result = await _identityService.CreateRoleAsync(request);
        return Ok(result);
    }
    
    [HttpPost("AssignRoles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Policy = "RequiredAdminManager")]
    public async Task<IActionResult> AssignRoles(AssignRoleRequest request)
    {
        var result = await _identityService.AssignRolesAsync(request);
        return Ok(result);
    }
}
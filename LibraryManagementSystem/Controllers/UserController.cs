using LibraryManagementSystem.Entities.DTOs;
using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("LoginUser")]
    public async Task<ActionResult> Login([FromBody] LoginViewModelDto model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<LoginResponseModelDto>.Fail("Invalid request.", "Error-400"));

        var result = await _userService.LoginUserAsync(model, ct).ConfigureAwait(false);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [AllowAnonymous]
    [HttpPost("RegisterUser")]
    public async Task<ActionResult> RegisterUser([FromBody] RegisterViewModelDto model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(MobileResponse<string>.Fail("Invalid request.", "Error-400"));

        var result = await _userService.RegisterUserAsync(model, ct).ConfigureAwait(false);
        return result.Status ? CreatedAtAction(nameof(RegisterUser), result) : BadRequest(result);
    }
}

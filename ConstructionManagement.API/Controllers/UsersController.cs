using ConstructionManagement.BLL.Services;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConstructionManagement.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> AddUser([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _userService.AddUserAsync(dto);
        return response.Success ? StatusCode(StatusCodes.Status201Created, response) : BadRequest(response);
    }

    [HttpGet]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager)]
    public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryDto query)
    {
        var currentRole = User.FindFirstValue(ClaimTypes.Role);
        if (string.Equals(currentRole, ApplicationRoles.ProjectManager, StringComparison.OrdinalIgnoreCase))
        {
            var normalizedFilter = ApplicationRoles.Normalize(query.Role);
            if (normalizedFilter != ApplicationRoles.Engineer)
            {
                return Forbid();
            }
        }

        var response = await _userService.GetAllUsersAsync(query);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var response = await _userService.GetUserByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _userService.UpdateUserAsync(id, dto);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var response = await _userService.DeleteUserAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateUserStatusDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _userService.UpdateUserStatusAsync(id, dto);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPatch("{id:guid}/role")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _userService.AssignRoleAsync(id, dto);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponseDto<object>.Fail("Invalid token subject."));
        }

        var response = await _userService.GetProfileAsync(userId);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponseDto<object>.Fail("Invalid token subject."));
        }

        var response = await _userService.UpdateProfileAsync(userId, dto);
        return response.Success ? Ok(response) : NotFound(response);
    }

    private bool TryGetUserId(out Guid userId)
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idValue, out userId);
    }
}

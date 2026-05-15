using ConstructionManagement.BLL.Services;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConstructionManagement.API.Controllers;

[ApiController]
[Route("api/sites")]
[Authorize]
public class SitesController : ControllerBase
{
    private readonly ISiteService _siteService;

    public SitesController(ISiteService siteService)
    {
        _siteService = siteService;
    }

    [HttpGet]
    [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.ProjectManager},{ApplicationRoles.Engineer},{ApplicationRoles.Accountant},{ApplicationRoles.Client}")]
    public async Task<IActionResult> GetAll([FromQuery] SiteQueryDto query)
    {
        if (!TryGetContext(out var userId, out var role, out var email))
            return Unauthorized(ApiResponseDto<object>.Fail("Invalid token."));

        var response = await _siteService.GetAllForUserAsync(query, userId, role!, email);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.ProjectManager},{ApplicationRoles.Engineer},{ApplicationRoles.Accountant},{ApplicationRoles.Client}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!TryGetContext(out var userId, out var role, out var email))
            return Unauthorized(ApiResponseDto<object>.Fail("Invalid token."));

        var response = await _siteService.GetByIdForUserAsync(id, userId, role!, email);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPost]
    [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.ProjectManager}")]
    public async Task<IActionResult> Create([FromBody] CreateSiteDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _siteService.CreateAsync(dto);
        return response.Success ? StatusCode(StatusCodes.Status201Created, response) : BadRequest(response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.ProjectManager}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSiteDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _siteService.UpdateAsync(id, dto);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _siteService.DeleteAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPatch("{id:guid}/assign-engineer")]
    [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.ProjectManager}")]
    public async Task<IActionResult> AssignEngineer(Guid id, [FromBody] AssignSiteEngineerDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _siteService.AssignEngineerAsync(id, dto);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.ProjectManager},{ApplicationRoles.Engineer}")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateSiteStatusDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (!TryGetContext(out var userId, out var role, out var email))
            return Unauthorized(ApiResponseDto<object>.Fail("Invalid token."));

        var response = await _siteService.UpdateStatusForUserAsync(id, dto, userId, role!, email);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpPatch("{id:guid}/progress")]
    [Authorize(Roles = $"{ApplicationRoles.Admin},{ApplicationRoles.ProjectManager},{ApplicationRoles.Engineer}")]
    public async Task<IActionResult> UpdateProgress(Guid id, [FromBody] UpdateSiteProgressDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        if (!TryGetContext(out var userId, out var role, out var email))
            return Unauthorized(ApiResponseDto<object>.Fail("Invalid token."));

        var response = await _siteService.UpdateProgressForUserAsync(id, dto, userId, role!, email);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    private bool TryGetContext(out Guid userId, out string? role, out string? email)
    {
        role = User.FindFirstValue(ClaimTypes.Role);
        email = User.FindFirstValue(ClaimTypes.Email);
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId) && !string.IsNullOrWhiteSpace(role);
    }
}

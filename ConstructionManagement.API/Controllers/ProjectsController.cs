using ConstructionManagement.BLL.Services;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConstructionManagement.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager + "," + ApplicationRoles.Engineer + "," + ApplicationRoles.Client)]
    public async Task<IActionResult> GetAll([FromQuery] ProjectQueryDto query)
    {
        if (!TryGetContext(out var userId, out var role, out var email))
            return Unauthorized(ApiResponseDto<object>.Fail("Invalid token."));

        var response = await _projectService.GetAllForUserAsync(query, userId, role!, email);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager + "," + ApplicationRoles.Engineer + "," + ApplicationRoles.Client)]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!TryGetContext(out var userId, out var role, out var email))
            return Unauthorized(ApiResponseDto<object>.Fail("Invalid token."));

        var response = await _projectService.GetByIdForUserAsync(id, userId, role!, email);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpGet("financial")]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager + "," + ApplicationRoles.Accountant)]
    public async Task<IActionResult> GetFinancial([FromQuery] ProjectQueryDto query)
    {
        if (!TryGetContext(out var userId, out var role, out var email))
            return Unauthorized(ApiResponseDto<object>.Fail("Invalid token."));

        var response = await _projectService.GetFinancialForUserAsync(query, userId, role!, email);
        return Ok(response);
    }

    [HttpGet("{id:guid}/financial")]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager + "," + ApplicationRoles.Accountant)]
    public async Task<IActionResult> GetFinancialById(Guid id)
    {
        if (!TryGetContext(out var userId, out var role, out var email))
            return Unauthorized(ApiResponseDto<object>.Fail("Invalid token."));

        var response = await _projectService.GetFinancialByIdForUserAsync(id, userId, role!, email);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _projectService.CreateAsync(dto);
        return response.Success ? StatusCode(StatusCodes.Status201Created, response) : BadRequest(response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _projectService.UpdateAsync(id, dto);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _projectService.DeleteAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateProjectStatusDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _projectService.UpdateStatusAsync(id, dto);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPatch("{id:guid}/progress")]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager)]
    public async Task<IActionResult> UpdateProgress(Guid id, [FromBody] UpdateProjectProgressDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var response = await _projectService.UpdateProgressAsync(id, dto);
        return response.Success ? Ok(response) : NotFound(response);
    }

    private bool TryGetContext(out Guid userId, out string? role, out string? email)
    {
        role = User.FindFirstValue(ClaimTypes.Role);
        email = User.FindFirstValue(ClaimTypes.Email);
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId) && !string.IsNullOrWhiteSpace(role);
    }
}

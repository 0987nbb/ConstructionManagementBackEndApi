using ConstructionManagement.BLL.Services;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionManagement.API.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Create([FromBody] CreateClientDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var response = await _clientService.CreateAsync(dto);
        return response.Success ? StatusCode(StatusCodes.Status201Created, response) : BadRequest(response);
    }

    [HttpGet]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager)] // PM read-only for project planning dropdowns
    public async Task<IActionResult> GetAll([FromQuery] ClientQueryDto query)
    {
        var response = await _clientService.GetAllAsync(query);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin + "," + ApplicationRoles.ProjectManager)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _clientService.GetByIdAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var response = await _clientService.UpdateAsync(id, dto);
        return response.Success ? Ok(response) : NotFound(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _clientService.DeleteAsync(id);
        return response.Success ? Ok(response) : NotFound(response);
    }

}

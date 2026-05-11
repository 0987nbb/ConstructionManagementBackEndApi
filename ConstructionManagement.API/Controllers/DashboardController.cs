using ConstructionManagement.BLL.Services;
using ConstructionManagement.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionManagement.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis()
    {
        var response = await _dashboardService.GetKpisAsync();
        return Ok(response);
    }
}

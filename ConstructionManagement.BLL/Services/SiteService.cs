using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Domain.Entities;
using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services;

public class SiteService : ISiteService
{
    private readonly ISiteRepository _siteRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;

    public SiteService(
        ISiteRepository siteRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IAuditService auditService)
    {
        _siteRepository = siteRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _auditService = auditService;
    }

    public async Task<ApiResponseDto<SiteDto>> CreateAsync(CreateSiteDto dto)
    {
        var status = NormalizeStatus(dto.Status);
        if (status == null) return ApiResponseDto<SiteDto>.Fail("Invalid site status.");

        var project = await _projectRepository.GetByIdWithDetailsAsync(dto.ProjectId);
        if (project == null) return ApiResponseDto<SiteDto>.Fail("Project not found.");

        var engineerError = await ValidateEngineerAsync(dto.AssignedEngineerId);
        if (engineerError != null) return ApiResponseDto<SiteDto>.Fail(engineerError);

        if (dto.EndDate.HasValue && dto.StartDate.HasValue && dto.EndDate.Value.Date < dto.StartDate.Value.Date)
            return ApiResponseDto<SiteDto>.Fail("End date cannot be earlier than start date.");

        var site = new Site
        {
            SiteName = dto.SiteName.Trim(),
            ProjectId = dto.ProjectId,
            Location = dto.Location.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            AssignedEngineerId = dto.AssignedEngineerId,
            Status = status,
            ProgressPercentage = dto.ProgressPercentage,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _siteRepository.AddAsync(site);
        await _siteRepository.SaveChangesAsync();

        var created = await _siteRepository.GetByIdWithDetailsAsync(site.Id);
        await _auditService.LogAsync(null, "site.created", null, new { site.Id, site.SiteName, site.ProjectId });

        return ApiResponseDto<SiteDto>.Ok(Map(created!), "Site created successfully.");
    }

    public async Task<ApiResponseDto<SiteDto>> UpdateAsync(Guid id, UpdateSiteDto dto)
    {
        var status = NormalizeStatus(dto.Status);
        if (status == null) return ApiResponseDto<SiteDto>.Fail("Invalid site status.");

        var site = await _siteRepository.GetByIdWithDetailsAsync(id);
        if (site == null) return ApiResponseDto<SiteDto>.Fail("Site not found.");

        var project = await _projectRepository.GetByIdWithDetailsAsync(dto.ProjectId);
        if (project == null) return ApiResponseDto<SiteDto>.Fail("Project not found.");

        var engineerError = await ValidateEngineerAsync(dto.AssignedEngineerId);
        if (engineerError != null) return ApiResponseDto<SiteDto>.Fail(engineerError);

        if (dto.EndDate.HasValue && dto.StartDate.HasValue && dto.EndDate.Value.Date < dto.StartDate.Value.Date)
            return ApiResponseDto<SiteDto>.Fail("End date cannot be earlier than start date.");

        site.SiteName = dto.SiteName.Trim();
        site.ProjectId = dto.ProjectId;
        site.Location = dto.Location.Trim();
        site.Latitude = dto.Latitude;
        site.Longitude = dto.Longitude;
        site.AssignedEngineerId = dto.AssignedEngineerId;
        site.Status = status;
        site.ProgressPercentage = dto.ProgressPercentage;
        site.StartDate = dto.StartDate;
        site.EndDate = dto.EndDate;
        site.Description = dto.Description?.Trim();
        site.UpdatedAt = DateTime.UtcNow;

        await _siteRepository.SaveChangesAsync();

        var updated = await _siteRepository.GetByIdWithDetailsAsync(site.Id);
        await _auditService.LogAsync(null, "site.updated", null, new { site.Id, site.SiteName });

        return ApiResponseDto<SiteDto>.Ok(Map(updated!), "Site updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(Guid id)
    {
        var site = await _siteRepository.GetByIdWithDetailsAsync(id);
        if (site == null) return ApiResponseDto<bool>.Fail("Site not found.");

        site.IsDeleted = true;
        site.DeletedAt = DateTime.UtcNow;
        site.UpdatedAt = DateTime.UtcNow;

        await _siteRepository.SaveChangesAsync();
        await _auditService.LogAsync(null, "site.deleted", null, new { site.Id, site.SiteName });

        return ApiResponseDto<bool>.Ok(true, "Site deleted successfully.");
    }

    public async Task<ApiResponseDto<SiteDto>> AssignEngineerAsync(Guid id, AssignSiteEngineerDto dto)
    {
        var site = await _siteRepository.GetByIdWithDetailsAsync(id);
        if (site == null) return ApiResponseDto<SiteDto>.Fail("Site not found.");

        var engineerError = await ValidateEngineerAsync(dto.AssignedEngineerId);
        if (engineerError != null) return ApiResponseDto<SiteDto>.Fail(engineerError);

        site.AssignedEngineerId = dto.AssignedEngineerId;
        site.UpdatedAt = DateTime.UtcNow;

        await _siteRepository.SaveChangesAsync();
        await _auditService.LogAsync(null, "site.engineer.assigned", null, new { site.Id, dto.AssignedEngineerId });

        var updated = await _siteRepository.GetByIdWithDetailsAsync(site.Id);
        return ApiResponseDto<SiteDto>.Ok(Map(updated!), "Engineer assignment updated successfully.");
    }

    public async Task<ApiResponseDto<SiteDto>> UpdateStatusForUserAsync(Guid id, UpdateSiteStatusDto dto, Guid userId, string role, string? email)
    {
        if (!RolePermissions.CanUpdateSiteStatus(role))
            return ApiResponseDto<SiteDto>.Fail("You are not allowed to update site status.");

        var status = NormalizeStatus(dto.Status);
        if (status == null) return ApiResponseDto<SiteDto>.Fail("Invalid site status.");

        var site = await _siteRepository.GetByIdWithDetailsAsync(id);
        if (site == null) return ApiResponseDto<SiteDto>.Fail("Site not found.");

        if (!CanEngineerExecuteOnSite(site, userId, role, email))
            return ApiResponseDto<SiteDto>.Fail("Site not found.");

        site.Status = status;
        site.UpdatedAt = DateTime.UtcNow;

        await _siteRepository.SaveChangesAsync();
        await _auditService.LogAsync(userId, "site.status.updated", null, new { site.Id, site.Status });

        return ApiResponseDto<SiteDto>.Ok(Map(site), "Site status updated successfully.");
    }

    public async Task<ApiResponseDto<SiteDto>> UpdateProgressForUserAsync(Guid id, UpdateSiteProgressDto dto, Guid userId, string role, string? email)
    {
        if (!RolePermissions.CanUpdateSiteProgress(role))
            return ApiResponseDto<SiteDto>.Fail("You are not allowed to update site progress.");

        var site = await _siteRepository.GetByIdWithDetailsAsync(id);
        if (site == null) return ApiResponseDto<SiteDto>.Fail("Site not found.");

        if (!CanEngineerExecuteOnSite(site, userId, role, email))
            return ApiResponseDto<SiteDto>.Fail("Site not found.");

        site.ProgressPercentage = dto.ProgressPercentage;
        site.UpdatedAt = DateTime.UtcNow;

        await _siteRepository.SaveChangesAsync();
        await _auditService.LogAsync(userId, "site.progress.updated", null, new { site.Id, site.ProgressPercentage });

        return ApiResponseDto<SiteDto>.Ok(Map(site), "Site progress updated successfully.");
    }

    public async Task<ApiResponseDto<List<SiteDto>>> GetAllForUserAsync(SiteQueryDto query, Guid userId, string role, string? email)
    {
        var sites = await _siteRepository.SearchActiveAsync(query.Search, NormalizeStatus(query.Status), query.ProjectId);
        var filtered = FilterSitesByRole(sites, userId, role, email);
        return ApiResponseDto<List<SiteDto>>.Ok(filtered.Select(Map).ToList());
    }

    public async Task<ApiResponseDto<SiteDto>> GetByIdForUserAsync(Guid id, Guid userId, string role, string? email)
    {
        var site = await _siteRepository.GetByIdWithDetailsAsync(id);
        if (site == null) return ApiResponseDto<SiteDto>.Fail("Site not found.");

        var allowed = FilterSitesByRole([site], userId, role, email).Any();
        if (!allowed) return ApiResponseDto<SiteDto>.Fail("Site not found.");

        return ApiResponseDto<SiteDto>.Ok(Map(site));
    }

    private static bool CanEngineerExecuteOnSite(Site site, Guid userId, string role, string? email)
    {
        if (role == ApplicationRoles.Admin || role == ApplicationRoles.ProjectManager)
            return true;

        if (role == ApplicationRoles.Engineer)
            return site.AssignedEngineerId == userId;

        return false;
    }

    private static IEnumerable<Site> FilterSitesByRole(IEnumerable<Site> sites, Guid userId, string role, string? email)
    {
        if (role == ApplicationRoles.Admin || role == ApplicationRoles.ProjectManager || role == ApplicationRoles.Accountant)
            return sites;

        if (role == ApplicationRoles.Engineer)
            return sites.Where(x => x.AssignedEngineerId == userId);

        if (role == ApplicationRoles.Client)
        {
            var normalized = email?.Trim().ToLowerInvariant();
            return sites.Where(x =>
                x.Project?.Client != null &&
                x.Project.Client.Email.ToLower() == normalized);
        }

        return [];
    }

    private async Task<string?> ValidateEngineerAsync(Guid? engineerId)
    {
        if (!engineerId.HasValue) return null;

        var engineer = await _userRepository.GetByIdActiveAsync(engineerId.Value);
        if (engineer == null || engineer.Role != ApplicationRoles.Engineer)
            return "Assigned engineer is invalid.";

        return null;
    }

    private static string? NormalizeStatus(string? status) => SiteStatuses.Normalize(status);

    private static SiteDto Map(Site site) => new()
    {
        Id = site.Id,
        SiteName = site.SiteName,
        ProjectId = site.ProjectId,
        ProjectName = site.Project?.ProjectName ?? string.Empty,
        Location = site.Location,
        Latitude = site.Latitude,
        Longitude = site.Longitude,
        AssignedEngineerId = site.AssignedEngineerId,
        AssignedEngineerName = site.AssignedEngineer?.FullName,
        Status = site.Status,
        ProgressPercentage = site.ProgressPercentage,
        StartDate = site.StartDate,
        EndDate = site.EndDate,
        Description = site.Description,
        CreatedAt = site.CreatedAt,
        UpdatedAt = site.UpdatedAt
    };
}

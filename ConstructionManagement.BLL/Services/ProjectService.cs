using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Constants;
using ConstructionManagement.Domain.Entities;
using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;

    public ProjectService(
        IProjectRepository projectRepository,
        IClientRepository clientRepository,
        IUserRepository userRepository,
        IAuditService auditService)
    {
        _projectRepository = projectRepository;
        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _auditService = auditService;
    }

    public async Task<ApiResponseDto<ProjectDto>> CreateAsync(CreateProjectDto dto)
    {
        var status = NormalizeStatus(dto.Status);
        if (status == null) return ApiResponseDto<ProjectDto>.Fail("Invalid project status.");

        var client = await _clientRepository.GetActiveByIdAsync(dto.ClientId);
        if (client == null) return ApiResponseDto<ProjectDto>.Fail("Client not found.");

        if (dto.AssignedEngineerId.HasValue)
        {
            var engineer = await _userRepository.GetByIdActiveAsync(dto.AssignedEngineerId.Value);
            if (engineer == null || engineer.Role != ApplicationRoles.Engineer)
                return ApiResponseDto<ProjectDto>.Fail("Assigned engineer is invalid.");
        }

        if (dto.EndDate.HasValue && dto.StartDate.HasValue && dto.EndDate.Value.Date < dto.StartDate.Value.Date)
            return ApiResponseDto<ProjectDto>.Fail("End date cannot be earlier than start date.");

        var project = new Project
        {
            ProjectName = dto.ProjectName.Trim(),
            Description = dto.Description?.Trim(),
            ClientId = dto.ClientId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Budget = dto.Budget,
            SpentAmount = 0,
            ProgressPercentage = dto.ProgressPercentage,
            Status = status,
            AssignedEngineerId = dto.AssignedEngineerId,
            CreatedAt = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);
        await _projectRepository.SaveChangesAsync();

        var created = await _projectRepository.GetByIdWithDetailsAsync(project.Id);
        await _auditService.LogAsync(null, "project.created", null, new { project.Id, project.ProjectName });

        return ApiResponseDto<ProjectDto>.Ok(Map(created!), "Project created successfully.");
    }

    public async Task<ApiResponseDto<ProjectDto>> UpdateAsync(Guid id, UpdateProjectDto dto)
    {
        var status = NormalizeStatus(dto.Status);
        if (status == null) return ApiResponseDto<ProjectDto>.Fail("Invalid project status.");

        var project = await _projectRepository.GetByIdWithDetailsAsync(id);
        if (project == null) return ApiResponseDto<ProjectDto>.Fail("Project not found.");

        var client = await _clientRepository.GetActiveByIdAsync(dto.ClientId);
        if (client == null) return ApiResponseDto<ProjectDto>.Fail("Client not found.");

        if (dto.AssignedEngineerId.HasValue)
        {
            var engineer = await _userRepository.GetByIdActiveAsync(dto.AssignedEngineerId.Value);
            if (engineer == null || engineer.Role != ApplicationRoles.Engineer)
                return ApiResponseDto<ProjectDto>.Fail("Assigned engineer is invalid.");
        }

        if (dto.EndDate.HasValue && dto.StartDate.HasValue && dto.EndDate.Value.Date < dto.StartDate.Value.Date)
            return ApiResponseDto<ProjectDto>.Fail("End date cannot be earlier than start date.");

        project.ProjectName = dto.ProjectName.Trim();
        project.Description = dto.Description?.Trim();
        project.ClientId = dto.ClientId;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.Budget = dto.Budget;
        project.SpentAmount = dto.SpentAmount;
        project.ProgressPercentage = dto.ProgressPercentage;
        project.Status = status;
        project.AssignedEngineerId = dto.AssignedEngineerId;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.SaveChangesAsync();

        var updated = await _projectRepository.GetByIdWithDetailsAsync(project.Id);
        await _auditService.LogAsync(null, "project.updated", null, new { project.Id, project.ProjectName });

        return ApiResponseDto<ProjectDto>.Ok(Map(updated!), "Project updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdWithDetailsAsync(id);
        if (project == null) return ApiResponseDto<bool>.Fail("Project not found.");

        project.IsDeleted = true;
        project.DeletedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.SaveChangesAsync();
        await _auditService.LogAsync(null, "project.deleted", null, new { project.Id, project.ProjectName });

        return ApiResponseDto<bool>.Ok(true, "Project deleted successfully.");
    }

    public async Task<ApiResponseDto<ProjectDto>> UpdateStatusAsync(Guid id, UpdateProjectStatusDto dto)
    {
        var status = NormalizeStatus(dto.Status);
        if (status == null) return ApiResponseDto<ProjectDto>.Fail("Invalid project status.");

        var project = await _projectRepository.GetByIdWithDetailsAsync(id);
        if (project == null) return ApiResponseDto<ProjectDto>.Fail("Project not found.");

        project.Status = status;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.SaveChangesAsync();
        await _auditService.LogAsync(null, "project.status.updated", null, new { project.Id, project.Status });

        return ApiResponseDto<ProjectDto>.Ok(Map(project), "Project status updated successfully.");
    }

    public async Task<ApiResponseDto<ProjectDto>> UpdateProgressAsync(Guid id, UpdateProjectProgressDto dto)
    {
        var project = await _projectRepository.GetByIdWithDetailsAsync(id);
        if (project == null) return ApiResponseDto<ProjectDto>.Fail("Project not found.");

        project.ProgressPercentage = dto.ProgressPercentage;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.SaveChangesAsync();
        await _auditService.LogAsync(null, "project.progress.updated", null, new { project.Id, project.ProgressPercentage });

        return ApiResponseDto<ProjectDto>.Ok(Map(project), "Project progress updated successfully.");
    }

    public async Task<ApiResponseDto<List<ProjectDto>>> GetAllForUserAsync(ProjectQueryDto query, Guid userId, string role, string? email)
    {
        var projects = await _projectRepository.SearchActiveAsync(query.Search, NormalizeStatus(query.Status), query.ClientId);
        var filtered = FilterProjectsByRole(projects, userId, role, email);
        var mapped = filtered.Select(Map).ToList();
        return ApiResponseDto<List<ProjectDto>>.Ok(mapped);
    }

    public async Task<ApiResponseDto<ProjectDto>> GetByIdForUserAsync(Guid id, Guid userId, string role, string? email)
    {
        var project = await _projectRepository.GetByIdWithDetailsAsync(id);
        if (project == null) return ApiResponseDto<ProjectDto>.Fail("Project not found.");

        var allowed = FilterProjectsByRole([project], userId, role, email).Any();
        if (!allowed) return ApiResponseDto<ProjectDto>.Fail("Project not found.");

        return ApiResponseDto<ProjectDto>.Ok(Map(project));
    }

    public async Task<ApiResponseDto<List<ProjectFinancialDto>>> GetFinancialForUserAsync(ProjectQueryDto query, Guid userId, string role, string? email)
    {
        var projects = await _projectRepository.SearchActiveAsync(query.Search, NormalizeStatus(query.Status), query.ClientId);
        var filtered = FilterProjectsByRole(projects, userId, role, email);
        var mapped = filtered.Select(MapFinancial).ToList();
        return ApiResponseDto<List<ProjectFinancialDto>>.Ok(mapped);
    }

    public async Task<ApiResponseDto<ProjectFinancialDto>> GetFinancialByIdForUserAsync(Guid id, Guid userId, string role, string? email)
    {
        var project = await _projectRepository.GetByIdWithDetailsAsync(id);
        if (project == null) return ApiResponseDto<ProjectFinancialDto>.Fail("Project not found.");

        var allowed = FilterProjectsByRole([project], userId, role, email).Any();
        if (!allowed) return ApiResponseDto<ProjectFinancialDto>.Fail("Project not found.");

        return ApiResponseDto<ProjectFinancialDto>.Ok(MapFinancial(project));
    }

    private static IEnumerable<Project> FilterProjectsByRole(IEnumerable<Project> projects, Guid userId, string role, string? email)
    {
        if (role == ApplicationRoles.Admin || role == ApplicationRoles.ProjectManager || role == ApplicationRoles.Accountant)
            return projects;

        if (role == ApplicationRoles.Engineer)
            return projects.Where(x => x.AssignedEngineerId == userId);

        if (role == ApplicationRoles.Client)
        {
            var normalized = email?.Trim().ToLowerInvariant();
            return projects.Where(x => x.Client != null && x.Client.Email.ToLower() == normalized);
        }

        return [];
    }

    private static string? NormalizeStatus(string? status) => ProjectStatuses.Normalize(status);

    private static ProjectDto Map(Project project) => new()
    {
        Id = project.Id,
        ProjectName = project.ProjectName,
        Description = project.Description,
        ClientId = project.ClientId,
        ClientName = project.Client?.Name ?? string.Empty,
        StartDate = project.StartDate,
        EndDate = project.EndDate,
        Budget = project.Budget,
        SpentAmount = project.SpentAmount,
        ProgressPercentage = project.ProgressPercentage,
        Status = project.Status,
        AssignedEngineerId = project.AssignedEngineerId,
        AssignedEngineerName = project.AssignedEngineer?.FullName,
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt
    };

    private static ProjectFinancialDto MapFinancial(Project project) => new()
    {
        Id = project.Id,
        ProjectName = project.ProjectName,
        ClientName = project.Client?.Name ?? string.Empty,
        Budget = project.Budget,
        SpentAmount = project.SpentAmount,
        RemainingAmount = project.Budget - project.SpentAmount,
        Status = project.Status
    };
}

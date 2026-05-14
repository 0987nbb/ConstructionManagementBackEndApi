using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Entities;
using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IAuditService _auditService;

    public ClientService(IClientRepository clientRepository, IProjectRepository projectRepository, IAuditService auditService)
    {
        _clientRepository = clientRepository;
        _projectRepository = projectRepository;
        _auditService = auditService;
    }

    public async Task<ApiResponseDto<ClientDto>> CreateAsync(CreateClientDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await _clientRepository.EmailExistsAsync(normalizedEmail))
        {
            return ApiResponseDto<ClientDto>.Fail("A client with this email already exists.");
        }

        var entity = new Client
        {
            Name = dto.Name.Trim(),
            Email = normalizedEmail,
            Phone = dto.Phone.Trim(),
            Address = dto.Address.Trim(),
            Company = dto.Company.Trim(),
            IsActive = dto.IsActive
        };

        await _clientRepository.AddAsync(entity);
        await _clientRepository.SaveChangesAsync();
        return ApiResponseDto<ClientDto>.Ok(Map(entity), "Client created successfully.");
    }

    public async Task<ApiResponseDto<List<ClientDto>>> GetAllAsync(ClientQueryDto query)
    {
        var clients = await _clientRepository.GetAllActiveAsync(query.Search, query.IsActive);
        return ApiResponseDto<List<ClientDto>>.Ok(clients.Select(Map).ToList());
    }

    public async Task<ApiResponseDto<ClientDto>> GetByIdAsync(Guid id)
    {
        var client = await _clientRepository.GetActiveByIdAsync(id);
        if (client == null)
        {
            return ApiResponseDto<ClientDto>.Fail("Client not found.");
        }

        return ApiResponseDto<ClientDto>.Ok(Map(client));
    }

    public async Task<ApiResponseDto<ClientDto>> UpdateAsync(Guid id, UpdateClientDto dto)
    {
        var client = await _clientRepository.GetActiveByIdAsync(id);
        if (client == null)
        {
            return ApiResponseDto<ClientDto>.Fail("Client not found.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (await _clientRepository.EmailExistsAsync(normalizedEmail, id))
        {
            return ApiResponseDto<ClientDto>.Fail("A client with this email already exists.");
        }

        client.Name = dto.Name.Trim();
        client.Email = normalizedEmail;
        client.Phone = dto.Phone.Trim();
        client.Address = dto.Address.Trim();
        client.Company = dto.Company.Trim();
        client.IsActive = dto.IsActive;
        client.UpdatedAt = DateTime.UtcNow;

        await _clientRepository.SaveChangesAsync();
        return ApiResponseDto<ClientDto>.Ok(Map(client), "Client updated successfully.");
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(Guid id)
    {
        var client = await _clientRepository.GetActiveByIdAsync(id);
        if (client == null)
        {
            return ApiResponseDto<bool>.Fail("Client not found.");
        }

        client.IsDeleted = true;
        client.IsActive = false;
        client.DeletedAt = DateTime.UtcNow;
        client.UpdatedAt = DateTime.UtcNow;
        await _clientRepository.SaveChangesAsync();
        return ApiResponseDto<bool>.Ok(true, "Client deleted successfully.");
    }

    public async Task<ApiResponseDto<ClientDto>> LinkProjectAsync(Guid clientId, LinkClientProjectDto dto)
    {
        var client = await _clientRepository.GetActiveByIdAsync(clientId);
        if (client == null)
        {
            return ApiResponseDto<ClientDto>.Fail("Client not found.");
        }

        var code = dto.ProjectCode.Trim().ToUpperInvariant();
        var project = await _projectRepository.GetByCodeAsync(code);

        if (project == null)
        {
            project = new Project
            {
                Name = dto.ProjectName.Trim(),
                Code = code,
                ClientId = clientId
            };

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();
        }
        else if (project.ClientId != clientId)
        {
            return ApiResponseDto<ClientDto>.Fail("Project is already assigned to a different client.");
        }

        if (client.Projects.All(p => p.Id != project.Id))
        {
            client.Projects.Add(project);
        }

        await _auditService.LogAsync(null, "client.project.linked", null, new { ClientId = clientId, ProjectCode = code });
        return ApiResponseDto<ClientDto>.Ok(Map(client), "Project linked to client successfully.");
    }

    private static ClientDto Map(Client client) => new()
    {
        Id = client.Id,
        Name = client.Name,
        Email = client.Email,
        Phone = client.Phone,
        Address = client.Address,
        Company = client.Company,
        IsActive = client.IsActive,
        CreatedAt = client.CreatedAt,
        UpdatedAt = client.UpdatedAt,
        Projects = client.Projects.Select(p => new ProjectLiteDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code
        }).ToList()
    };
}

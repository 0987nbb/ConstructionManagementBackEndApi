using ConstructionManagement.Dtos;

namespace ConstructionManagement.BLL.Services;

public interface IClientService
{
    Task<ApiResponseDto<ClientDto>> CreateAsync(CreateClientDto dto);
    Task<ApiResponseDto<List<ClientDto>>> GetAllAsync(ClientQueryDto query);
    Task<ApiResponseDto<ClientDto>> GetByIdAsync(Guid id);
    Task<ApiResponseDto<ClientDto>> UpdateAsync(Guid id, UpdateClientDto dto);
    Task<ApiResponseDto<bool>> DeleteAsync(Guid id);
    Task<ApiResponseDto<ClientDto>> LinkProjectAsync(Guid clientId, LinkClientProjectDto dto);
}

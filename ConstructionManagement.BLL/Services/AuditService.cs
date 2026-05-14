using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Entities;
using System.Text.Json;

namespace ConstructionManagement.BLL.Services;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string action, string? ipAddress = null, object? metadata = null);
}

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(Guid? userId, string action, string? ipAddress = null, object? metadata = null)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            IpAddress = ipAddress,
            Metadata = metadata == null ? null : JsonSerializer.Serialize(metadata),
            TimestampUtc = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(log);
        await _auditLogRepository.SaveChangesAsync();
    }
}

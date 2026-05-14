using ConstructionManagement.DAL.Data;
using ConstructionManagement.DAL.Repositories.Interfaces;
using ConstructionManagement.Domain.Entities;

namespace ConstructionManagement.DAL.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext context) : base(context)
    {
    }
}

using Assignmen_PRN232__.Models;
using Assignmen_PRN232_1.Services.IServices;

namespace Assignmen_PRN232_1.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _db;

    public AuditLogService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        string action,
        string entityType,
        string? entityId,
        string? dataBefore,
        string? dataAfter,
        short? performedById,
        string? performedByName)
    {
        var entry = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            DataBefore = dataBefore,
            DataAfter = dataAfter,
            PerformedById = performedById,
            PerformedByName = performedByName,
            Timestamp = DateTime.UtcNow
        };

        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync();
    }
}

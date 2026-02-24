using Assignmen_PRN232__.Models;

namespace Assignmen_PRN232_1.Services.IServices;

public interface IAuditLogService
{
    Task LogAsync(
        string action,
        string entityType,
        string? entityId,
        string? dataBefore,
        string? dataAfter,
        short? performedById,
        string? performedByName);
}

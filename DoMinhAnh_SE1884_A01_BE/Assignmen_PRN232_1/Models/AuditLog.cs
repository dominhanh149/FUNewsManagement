namespace Assignmen_PRN232__.Models;

public class AuditLog
{
    public int Id { get; set; }

    /// <summary>Action type: Create, Update, Delete</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Name of the entity that was changed, e.g. "NewsArticle"</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Primary key of the affected entity</summary>
    public string? EntityId { get; set; }

    /// <summary>JSON snapshot BEFORE the change (null for Create)</summary>
    public string? DataBefore { get; set; }

    /// <summary>JSON snapshot AFTER the change (null for Delete)</summary>
    public string? DataAfter { get; set; }

    /// <summary>Account that performed the action</summary>
    public short? PerformedById { get; set; }

    public string? PerformedByName { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

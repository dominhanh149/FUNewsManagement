using System;
using System.Collections.Generic;

namespace AnalyticsAPI.Models;

public partial class RefreshToken
{
    public long Id { get; set; }

    public short AccountId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByToken { get; set; }

    public virtual SystemAccount Account { get; set; } = null!;
}

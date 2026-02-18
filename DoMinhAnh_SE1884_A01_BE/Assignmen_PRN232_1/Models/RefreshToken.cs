namespace Assignmen_PRN232__.Models;

public class RefreshToken
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

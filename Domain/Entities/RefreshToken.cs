namespace Domain.Entities;

public class RefreshToken : BaseAuditableEntity
{
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public DateTime? Revoked { get; set; }
    public string RevokedByIp { get; set; }
    public string CreatedByIp { get; set; }
    public string DeviceInfo { get; set; }
    public bool IsActive => !IsExpired && !Revoked.HasValue;
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
}
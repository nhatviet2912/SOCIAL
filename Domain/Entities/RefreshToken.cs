namespace Domain.Entities;

public class RefreshToken : BaseAuditableEntity
{
    public string Token { get; set; } = null!;
    public DateTime Expires { get; set; }
    public Guid UserId { get; set; }
    public virtual ApplicationUser User { get; set; } = null!;
}
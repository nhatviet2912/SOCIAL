using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
{
    public virtual ApplicationRole Role { get; set; }
}
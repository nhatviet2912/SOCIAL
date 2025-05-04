using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public virtual ICollection<ApplicationUserClaim> Claims { get; set; } = null!;
    public virtual ICollection<ApplicationUserLogin> Logins { get; set; } = null!;
    public virtual ICollection<ApplicationUserToken> Tokens { get; set; } = null!;
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = null!;
    
    public Guid MemberId { get; set; }
    public virtual Member Member { get; set; } = null!;
}
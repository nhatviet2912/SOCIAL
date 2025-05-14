using System.Reflection;
using Application.Common.Interfaces.Service;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, 
    ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>
{
    private readonly IUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        IUserService currentUserService,
        IDateTimeService dateTimeService)
        : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTimeService.GetCurrentDateTimeAsync();
        var currentUserId = _currentUserService.UserId ?? null;

        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.Created = now;
                entry.Entity.CreatedBy = currentUserId;
                entry.Entity.LastModified = now;
                entry.Entity.LastModifiedBy = currentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModified = now;
                entry.Entity.LastModifiedBy = currentUserId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<ApplicationUserRole>(userRole =>
        {
            userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

            userRole.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            userRole.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });
        
        builder.Entity<ApplicationUserClaim>(userClaim =>
        {
            userClaim.HasOne(uc => uc.User)
                .WithMany(u => u.Claims)
                .HasForeignKey(uc => uc.UserId)
                .IsRequired();
        });
        
        builder.Entity<ApplicationUserLogin>(userLogin =>
        {
            userLogin.HasKey(l => new { l.LoginProvider, l.ProviderKey });

            userLogin.HasOne(l => l.User)
                .WithMany(u => u.Logins)
                .HasForeignKey(l => l.UserId)
                .IsRequired();
        });

        builder.Entity<ApplicationUserToken>(userToken =>
        {
            userToken.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            userToken.HasOne(t => t.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(t => t.UserId)
                .IsRequired();
        });

        builder.Entity<ApplicationRoleClaim>(roleClaim =>
        {
            roleClaim.HasOne(rc => rc.Role)
                .WithMany(r => r.RoleClaims)
                .HasForeignKey(rc => rc.RoleId)
                .IsRequired();
        });
        
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Member)
            .WithOne()
            .HasForeignKey<ApplicationUser>(u => u.MemberId)
            .IsRequired(false);
        
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    
    public DbSet<Member> Members { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}
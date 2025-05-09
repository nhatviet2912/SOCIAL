namespace Application.DTO.Request.Role;

public class AssignRoleRequest
{
    public Guid UserId { get; set; }
    public string[]? RoleNames { get; set; }
}
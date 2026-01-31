namespace PostTrade.Domain.Entities.MasterData;

public class RolePermission : BaseEntity
{
    public Guid RolePermissionId { get; set; }
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    
    // Navigation
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}

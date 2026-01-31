namespace PostTrade.Domain.Entities.MasterData;

public class UserRole : BaseEntity
{
    public Guid UserRoleId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedDate { get; set; }
    
    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}

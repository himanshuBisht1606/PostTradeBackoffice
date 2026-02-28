namespace PostTrade.Domain.Entities.MasterData;

public class ClientJointHolder : BaseEntity
{
    public Guid JointHolderId { get; set; }
    public Guid ClientId { get; set; }
    public Guid TenantId { get; set; }
    public int HolderNumber { get; set; } // 2 or 3
    public string Pan { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Relationship { get; set; } = string.Empty;

    public virtual Client Client { get; set; } = null!;
}

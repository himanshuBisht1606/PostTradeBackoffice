namespace PostTrade.Application.Interfaces;
public interface ITenantContext
{
    Guid GetCurrentTenantId();
    void SetTenantId(Guid tenantId);
}

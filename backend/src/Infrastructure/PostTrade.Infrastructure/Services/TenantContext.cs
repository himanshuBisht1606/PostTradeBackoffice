using PostTrade.Application.Interfaces;

namespace PostTrade.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private Guid _tenantId;

    public Guid GetCurrentTenantId()
    {
        if (_tenantId == Guid.Empty)
            throw new InvalidOperationException("Tenant context not set");
        return _tenantId;
    }

    public void SetTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
    }
}

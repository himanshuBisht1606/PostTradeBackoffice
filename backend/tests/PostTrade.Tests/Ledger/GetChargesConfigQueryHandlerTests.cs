using PostTrade.Application.Features.Ledger.Charges.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Ledger;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Ledger;

public class GetChargesConfigQueryHandlerTests
{
    private readonly Mock<IRepository<ChargesConfiguration>> _repo          = new();
    private readonly Mock<ITenantContext>                    _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private GetChargesConfigQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetChargesConfigQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static ChargesConfiguration MakeConfig(ChargeType chargeType = ChargeType.Brokerage, bool isActive = true) => new()
    {
        ChargesConfigId = Guid.NewGuid(),
        TenantId        = TenantId,
        BrokerId        = Guid.NewGuid(),
        ChargeName      = chargeType.ToString(),
        ChargeType      = chargeType,
        CalculationType = CalculationType.Percentage,
        Rate            = 0.5m,
        MinAmount       = 50m,
        MaxAmount       = 5000m,
        IsActive        = isActive,
        EffectiveFrom   = DateTime.Today
    };

    [Fact]
    public async Task Handle_ShouldReturnAllConfigsForTenant()
    {
        var configs = new List<ChargesConfiguration> { MakeConfig(), MakeConfig(ChargeType.STT) };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ChargesConfiguration, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(configs);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetChargesConfigQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoConfigs_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ChargesConfiguration, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ChargesConfiguration>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetChargesConfigQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteringByChargeType_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ChargesConfiguration, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ChargesConfiguration>());

        var handler = CreateHandler();
        await handler.Handle(new GetChargesConfigQuery(ChargeType: ChargeType.Brokerage), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<ChargesConfiguration, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ChargesConfiguration, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ChargesConfiguration>());

        var handler = CreateHandler();
        await handler.Handle(new GetChargesConfigQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}

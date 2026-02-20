using PostTrade.Application.Features.Trading.Positions.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Tests.Trading.Positions;

public class GetPositionsByClientQueryHandlerTests
{
    private readonly Mock<IRepository<Position>> _repo          = new();
    private readonly Mock<ITenantContext>        _tenantContext = new();

    private static readonly Guid TenantId     = Guid.NewGuid();
    private static readonly Guid ClientId     = Guid.NewGuid();
    private static readonly Guid InstrumentId = Guid.NewGuid();

    private GetPositionsByClientQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetPositionsByClientQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static Position MakePosition(Guid? clientId = null) => new()
    {
        PositionId      = Guid.NewGuid(),
        TenantId        = TenantId,
        BrokerId        = Guid.NewGuid(),
        ClientId        = clientId ?? ClientId,
        InstrumentId    = InstrumentId,
        PositionDate    = DateTime.Today,
        BuyQuantity     = 100,
        NetQuantity     = 100,
        AverageBuyPrice = 250m,
        MarketPrice     = 260m
    };

    [Fact]
    public async Task Handle_ShouldReturnPositionsForSpecifiedClient()
    {
        var positions = new List<Position> { MakePosition(), MakePosition() };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(positions);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetPositionsByClientQuery(ClientId), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenClientHasNoPositions_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Position>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetPositionsByClientQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Position>());

        var handler = CreateHandler();
        await handler.Handle(new GetPositionsByClientQuery(ClientId), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}

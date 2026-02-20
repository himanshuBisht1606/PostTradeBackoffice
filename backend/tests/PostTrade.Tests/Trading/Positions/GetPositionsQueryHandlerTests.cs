using PostTrade.Application.Features.Trading.Positions.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Trading;

namespace PostTrade.Tests.Trading.Positions;

public class GetPositionsQueryHandlerTests
{
    private readonly Mock<IRepository<Position>> _repo          = new();
    private readonly Mock<ITenantContext>        _tenantContext = new();

    private static readonly Guid TenantId     = Guid.NewGuid();
    private static readonly Guid ClientId     = Guid.NewGuid();
    private static readonly Guid InstrumentId = Guid.NewGuid();

    private GetPositionsQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetPositionsQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static Position MakePosition(int netQty = 100) => new()
    {
        PositionId      = Guid.NewGuid(),
        TenantId        = TenantId,
        BrokerId        = Guid.NewGuid(),
        ClientId        = ClientId,
        InstrumentId    = InstrumentId,
        PositionDate    = DateTime.Today,
        BuyQuantity     = netQty > 0 ? netQty : 0,
        SellQuantity    = netQty < 0 ? -netQty : 0,
        NetQuantity     = netQty,
        AverageBuyPrice = 250m,
        MarketPrice     = 260m,
        TotalPnL        = 1000m
    };

    [Fact]
    public async Task Handle_ShouldReturnOpenPositions()
    {
        var positions = new List<Position> { MakePosition(100), MakePosition(50) };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(positions);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetPositionsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoOpenPositions_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Position>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetPositionsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Position>());

        var handler = CreateHandler();
        await handler.Handle(new GetPositionsQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapAllDtoFieldsCorrectly()
    {
        var position = MakePosition(100);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Position, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Position> { position });

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetPositionsQuery(), CancellationToken.None)).First();

        result.PositionId.Should().Be(position.PositionId);
        result.TenantId.Should().Be(position.TenantId);
        result.ClientId.Should().Be(position.ClientId);
        result.InstrumentId.Should().Be(position.InstrumentId);
        result.NetQuantity.Should().Be(position.NetQuantity);
        result.TotalPnL.Should().Be(position.TotalPnL);
    }
}

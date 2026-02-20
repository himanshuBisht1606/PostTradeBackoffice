using PostTrade.Application.Features.MasterSetup.Exchanges.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;

namespace PostTrade.Tests.MasterSetup.Exchanges;

public class GetExchangesQueryHandlerTests
{
    private readonly Mock<IRepository<Exchange>> _repo          = new();
    private readonly Mock<ITenantContext>        _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private GetExchangesQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetExchangesQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static Exchange MakeExchange(int index = 1) => new()
    {
        ExchangeId   = Guid.NewGuid(),
        TenantId     = TenantId,
        ExchangeCode = $"EXC{index:D3}",
        ExchangeName = $"Exchange {index}",
        Country      = "India",
        IsActive     = true
    };

    [Fact]
    public async Task Handle_ShouldReturnAllExchangesForTenant()
    {
        var exchanges = new List<Exchange> { MakeExchange(1), MakeExchange(2) };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Exchange, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(exchanges);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetExchangesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoExchanges_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Exchange, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Exchange>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetExchangesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapAllDtoFieldsCorrectly()
    {
        var exchange = MakeExchange(1);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Exchange, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Exchange> { exchange });

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetExchangesQuery(), CancellationToken.None)).First();

        result.ExchangeId.Should().Be(exchange.ExchangeId);
        result.TenantId.Should().Be(exchange.TenantId);
        result.ExchangeCode.Should().Be(exchange.ExchangeCode);
        result.ExchangeName.Should().Be(exchange.ExchangeName);
        result.Country.Should().Be(exchange.Country);
        result.IsActive.Should().BeTrue();
    }
}

using PostTrade.Application.Features.MasterSetup.Instruments.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Instruments;

public class GetInstrumentsQueryHandlerTests
{
    private readonly Mock<IRepository<Instrument>> _repo          = new();
    private readonly Mock<ITenantContext>          _tenantContext = new();

    private static readonly Guid TenantId   = Guid.NewGuid();
    private static readonly Guid ExchangeId = Guid.NewGuid();
    private static readonly Guid SegmentId  = Guid.NewGuid();

    private GetInstrumentsQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetInstrumentsQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static Instrument MakeInstrument(int index = 1, InstrumentType type = InstrumentType.Equity) => new()
    {
        InstrumentId   = Guid.NewGuid(),
        TenantId       = TenantId,
        ExchangeId     = ExchangeId,
        SegmentId      = SegmentId,
        InstrumentCode = $"INST{index:D3}",
        InstrumentName = $"Instrument {index}",
        Symbol         = $"SYM{index:D3}",
        InstrumentType = type,
        LotSize        = 1m,
        TickSize       = 0.05m,
        Status         = InstrumentStatus.Active
    };

    [Fact]
    public async Task Handle_ShouldReturnAllInstrumentsForTenant()
    {
        var instruments = new List<Instrument> { MakeInstrument(1), MakeInstrument(2) };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Instrument, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(instruments);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetInstrumentsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoInstruments_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Instrument, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Instrument>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetInstrumentsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteringByExchangeId_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Instrument, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Instrument>());

        var handler = CreateHandler();
        await handler.Handle(new GetInstrumentsQuery(ExchangeId: ExchangeId), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Instrument, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFilteringByType_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Instrument, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Instrument>());

        var handler = CreateHandler();
        await handler.Handle(new GetInstrumentsQuery(Type: InstrumentType.Equity), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<Instrument, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapAllDtoFieldsCorrectly()
    {
        var instrument = MakeInstrument(1);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Instrument, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Instrument> { instrument });

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetInstrumentsQuery(), CancellationToken.None)).First();

        result.InstrumentId.Should().Be(instrument.InstrumentId);
        result.TenantId.Should().Be(instrument.TenantId);
        result.InstrumentCode.Should().Be(instrument.InstrumentCode);
        result.Symbol.Should().Be(instrument.Symbol);
        result.InstrumentType.Should().Be(instrument.InstrumentType);
        result.LotSize.Should().Be(instrument.LotSize);
        result.Status.Should().Be(instrument.Status);
    }
}

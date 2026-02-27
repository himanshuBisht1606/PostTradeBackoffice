using PostTrade.Application.Features.MasterSetup.Instruments.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Instruments;

public class UpdateInstrumentCommandHandlerTests
{
    private readonly Mock<IRepository<Instrument>> _repo          = new();
    private readonly Mock<IUnitOfWork>             _unitOfWork    = new();
    private readonly Mock<ITenantContext>          _tenantContext = new();

    private static readonly Guid TenantId   = Guid.NewGuid();
    private static readonly Guid ExchangeId = Guid.NewGuid();
    private static readonly Guid SegmentId  = Guid.NewGuid();

    private UpdateInstrumentCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new UpdateInstrumentCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private Instrument ExistingInstrument(Guid instrumentId) => new()
    {
        InstrumentId   = instrumentId,
        TenantId       = TenantId,
        ExchangeId     = ExchangeId,
        SegmentId      = SegmentId,
        InstrumentCode = "RELIANCE",
        InstrumentName = "Reliance Industries",
        Symbol         = "RELIANCE",
        InstrumentType = InstrumentType.Equity,
        LotSize        = 1m,
        TickSize       = 0.05m,
        Status         = InstrumentStatus.Active
    };

    private static UpdateInstrumentCommand ValidCommand(Guid instrumentId) => new(
        InstrumentId:   instrumentId,
        InstrumentName: "Reliance Industries Updated",
        LotSize:        5m,
        TickSize:       0.10m,
        Status:         InstrumentStatus.Active,
        ExpiryDate:     null,
        StrikePrice:    null
    );

    [Fact]
    public async Task Handle_WhenInstrumentNotFound_ShouldReturnNull()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Instrument, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Instrument>());

        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInstrumentExists_ShouldUpdateFields()
    {
        var instrumentId = Guid.NewGuid();
        var instrument   = ExistingInstrument(instrumentId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Instrument, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Instrument> { instrument });

        var command = ValidCommand(instrumentId);
        var handler = CreateHandler();
        var result  = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.InstrumentName.Should().Be(command.InstrumentName);
        result.LotSize.Should().Be(command.LotSize);
        result.TickSize.Should().Be(command.TickSize);
    }

    [Fact]
    public async Task Handle_WhenInstrumentExists_ShouldCallUpdateAndSave()
    {
        var instrumentId = Guid.NewGuid();
        var instrument   = ExistingInstrument(instrumentId);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Instrument, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Instrument> { instrument });

        var handler = CreateHandler();
        await handler.Handle(ValidCommand(instrumentId), CancellationToken.None);

        _repo.Verify(r => r.UpdateAsync(instrument, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotChangeInstrumentCodeOrSymbolOrType()
    {
        var instrumentId   = Guid.NewGuid();
        var instrument     = ExistingInstrument(instrumentId);
        var originalCode   = instrument.InstrumentCode;
        var originalSymbol = instrument.Symbol;
        var originalType   = instrument.InstrumentType;

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Instrument, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Instrument> { instrument });

        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(instrumentId), CancellationToken.None);

        result!.InstrumentCode.Should().Be(originalCode);
        result.Symbol.Should().Be(originalSymbol);
        result.InstrumentType.Should().Be(originalType);
    }
}

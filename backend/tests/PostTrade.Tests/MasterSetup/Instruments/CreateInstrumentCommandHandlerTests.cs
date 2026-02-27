using PostTrade.Application.Features.MasterSetup.Instruments.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Instruments;

public class CreateInstrumentCommandHandlerTests
{
    private readonly Mock<IRepository<Instrument>> _repo          = new();
    private readonly Mock<IUnitOfWork>             _unitOfWork    = new();
    private readonly Mock<ITenantContext>          _tenantContext = new();

    private static readonly Guid TenantId   = Guid.NewGuid();
    private static readonly Guid ExchangeId = Guid.NewGuid();
    private static readonly Guid SegmentId  = Guid.NewGuid();

    private CreateInstrumentCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repo.Setup(r => r.AddAsync(It.IsAny<Instrument>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((Instrument i, CancellationToken _) => i);
        return new CreateInstrumentCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private static CreateInstrumentCommand ValidCommand() => new(
        InstrumentCode: "RELIANCE",
        InstrumentName: "Reliance Industries",
        Symbol:         "RELIANCE",
        ISIN:           "INE002A01018",
        ExchangeId:     ExchangeId,
        SegmentId:      SegmentId,
        InstrumentType: InstrumentType.Equity,
        LotSize:        1m,
        TickSize:       0.05m,
        Series:         "EQ",
        ExpiryDate:     null,
        StrikePrice:    null,
        OptionType:     null
    );

    [Fact]
    public async Task Handle_ShouldSetStatusToActive()
    {
        Instrument? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Instrument>(), It.IsAny<CancellationToken>()))
             .Callback<Instrument, CancellationToken>((i, _) => captured = i)
             .ReturnsAsync((Instrument i, CancellationToken _) => i);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Status.Should().Be(InstrumentStatus.Active);
    }

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        Instrument? captured = null;
        var handler = CreateHandler();

        _repo.Setup(r => r.AddAsync(It.IsAny<Instrument>(), It.IsAny<CancellationToken>()))
             .Callback<Instrument, CancellationToken>((i, _) => captured = i)
             .ReturnsAsync((Instrument i, CancellationToken _) => i);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewInstrumentId()
    {
        var handler = CreateHandler();
        var result  = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.InstrumentId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<Instrument>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithCorrectFields()
    {
        var command = ValidCommand();
        var handler = CreateHandler();

        var result = await handler.Handle(command, CancellationToken.None);

        result.TenantId.Should().Be(TenantId);
        result.InstrumentCode.Should().Be(command.InstrumentCode);
        result.InstrumentName.Should().Be(command.InstrumentName);
        result.Symbol.Should().Be(command.Symbol);
        result.ExchangeId.Should().Be(command.ExchangeId);
        result.SegmentId.Should().Be(command.SegmentId);
        result.InstrumentType.Should().Be(command.InstrumentType);
        result.LotSize.Should().Be(command.LotSize);
        result.TickSize.Should().Be(command.TickSize);
        result.Status.Should().Be(InstrumentStatus.Active);
    }
}

using PostTrade.Application.Features.Ledger.Charges.Commands;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Ledger;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Ledger;

public class CreateChargesConfigCommandHandlerTests
{
    private readonly Mock<IRepository<ChargesConfiguration>> _repo          = new();
    private readonly Mock<IUnitOfWork>                       _unitOfWork    = new();
    private readonly Mock<ITenantContext>                    _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid BrokerId = Guid.NewGuid();

    private CreateChargesConfigCommandHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _repo.Setup(r => r.AddAsync(It.IsAny<ChargesConfiguration>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((ChargesConfiguration c, CancellationToken _) => c);
        return new CreateChargesConfigCommandHandler(_repo.Object, _unitOfWork.Object, _tenantContext.Object);
    }

    private static CreateChargesConfigCommand ValidCommand() => new(
        BrokerId:        BrokerId,
        ChargeName:      "Brokerage",
        ChargeType:      ChargeType.Brokerage,
        CalculationType: CalculationType.Percentage,
        Rate:            0.5m,
        MinAmount:       50m,
        MaxAmount:       5000m,
        EffectiveFrom:   DateTime.Today,
        EffectiveTo:     DateTime.Today.AddYears(1)
    );

    [Fact]
    public async Task Handle_ShouldSetIsActiveToTrue()
    {
        ChargesConfiguration? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<ChargesConfiguration>(), It.IsAny<CancellationToken>()))
             .Callback<ChargesConfiguration, CancellationToken>((c, _) => captured = c)
             .ReturnsAsync((ChargesConfiguration c, CancellationToken _) => c);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldAssignCurrentTenantId()
    {
        ChargesConfiguration? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<ChargesConfiguration>(), It.IsAny<CancellationToken>()))
             .Callback<ChargesConfiguration, CancellationToken>((c, _) => captured = c)
             .ReturnsAsync((ChargesConfiguration c, CancellationToken _) => c);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public async Task Handle_ShouldGenerateChargesConfigId()
    {
        ChargesConfiguration? captured = null;
        var handler = CreateHandler();
        _repo.Setup(r => r.AddAsync(It.IsAny<ChargesConfiguration>(), It.IsAny<CancellationToken>()))
             .Callback<ChargesConfiguration, CancellationToken>((c, _) => captured = c)
             .ReturnsAsync((ChargesConfiguration c, CancellationToken _) => c);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        captured!.ChargesConfigId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncAndSaveChanges()
    {
        var handler = CreateHandler();
        await handler.Handle(ValidCommand(), CancellationToken.None);

        _repo.Verify(r => r.AddAsync(It.IsAny<ChargesConfiguration>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDtoWithCorrectFields()
    {
        var command = ValidCommand();
        var handler = CreateHandler();
        var result  = await handler.Handle(command, CancellationToken.None);

        result.TenantId.Should().Be(TenantId);
        result.ChargeName.Should().Be(command.ChargeName);
        result.ChargeType.Should().Be(command.ChargeType);
        result.Rate.Should().Be(command.Rate);
        result.IsActive.Should().BeTrue();
    }
}

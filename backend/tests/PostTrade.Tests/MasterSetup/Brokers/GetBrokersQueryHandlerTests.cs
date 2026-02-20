using PostTrade.Application.Features.MasterSetup.Brokers.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Brokers;

public class GetBrokersQueryHandlerTests
{
    private readonly Mock<IRepository<Broker>> _repo          = new();
    private readonly Mock<ITenantContext>      _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private GetBrokersQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetBrokersQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static Broker MakeBroker(int index = 1) => new()
    {
        BrokerId     = Guid.NewGuid(),
        TenantId     = TenantId,
        BrokerCode   = $"BRK00{index}",
        BrokerName   = $"Broker {index}",
        ContactEmail = $"broker{index}@corp.com",
        ContactPhone = "9876543210",
        Status       = BrokerStatus.Active
    };

    [Fact]
    public async Task Handle_ShouldReturnAllBrokersForTenant()
    {
        var brokers = new List<Broker> { MakeBroker(1), MakeBroker(2) };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Broker, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(brokers);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetBrokersQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoBrokers_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Broker, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Broker>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetBrokersQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Broker, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Broker>());

        var handler = CreateHandler();
        await handler.Handle(new GetBrokersQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapAllDtoFieldsCorrectly()
    {
        var broker = MakeBroker(1);

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Broker, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Broker> { broker });

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetBrokersQuery(), CancellationToken.None)).First();

        result.BrokerId.Should().Be(broker.BrokerId);
        result.TenantId.Should().Be(broker.TenantId);
        result.BrokerCode.Should().Be(broker.BrokerCode);
        result.BrokerName.Should().Be(broker.BrokerName);
        result.Status.Should().Be(broker.Status);
    }
}

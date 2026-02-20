using PostTrade.Application.Features.MasterSetup.Clients.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Clients;

public class GetClientByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Client>> _repo         = new();
    private readonly Mock<ITenantContext>      _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private GetClientByIdQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetClientByIdQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private Client ExistingClient(Guid clientId) => new()
    {
        ClientId   = clientId,
        TenantId   = TenantId,
        BrokerId   = Guid.NewGuid(),
        ClientCode = "CLT001",
        ClientName = "John Doe",
        Email      = "john@example.com",
        Phone      = "9876543210",
        ClientType = ClientType.Individual,
        Status     = ClientStatus.Active
    };

    [Fact]
    public async Task Handle_WhenClientExists_ShouldReturnDto()
    {
        var clientId = Guid.NewGuid();
        var client   = ExistingClient(clientId);

        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client> { client });

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetClientByIdQuery(clientId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ClientId.Should().Be(clientId);
        result.ClientName.Should().Be(client.ClientName);
        result.Email.Should().Be(client.Email);
    }

    [Fact]
    public async Task Handle_WhenClientNotFound_ShouldReturnNull()
    {
        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetClientByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantIdForQuery()
    {
        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client>());

        var handler = CreateHandler();
        await handler.Handle(new GetClientByIdQuery(Guid.NewGuid()), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}

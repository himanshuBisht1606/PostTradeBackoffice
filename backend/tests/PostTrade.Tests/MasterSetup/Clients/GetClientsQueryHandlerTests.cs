using PostTrade.Application.Features.MasterSetup.Clients.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.MasterData;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.MasterSetup.Clients;

public class GetClientsQueryHandlerTests
{
    private readonly Mock<IRepository<Client>> _repo         = new();
    private readonly Mock<ITenantContext>      _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();

    private GetClientsQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetClientsQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static Client MakeClient(int index = 1) => new()
    {
        ClientId   = Guid.NewGuid(),
        TenantId   = TenantId,
        BrokerId   = Guid.NewGuid(),
        ClientCode = $"CLT00{index}",
        ClientName = $"Client {index}",
        Email      = $"client{index}@example.com",
        Phone      = "9876543210",
        ClientType = ClientType.Individual,
        Status     = ClientStatus.Active
    };

    [Fact]
    public async Task Handle_ShouldReturnAllClientsForTenant()
    {
        var clients = new List<Client> { MakeClient(1), MakeClient(2), MakeClient(3) };

        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clients);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetClientsQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_ShouldApplyPageSizeCorrectly()
    {
        var clients = Enumerable.Range(1, 25).Select(MakeClient).ToList();

        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clients);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetClientsQuery(Page: 1, PageSize: 10), CancellationToken.None);

        result.Should().HaveCount(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectPageOnPage2()
    {
        var clients = Enumerable.Range(1, 25).Select(MakeClient).ToList();

        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clients);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetClientsQuery(Page: 2, PageSize: 10), CancellationToken.None);

        result.Should().HaveCount(10);
    }

    [Fact]
    public async Task Handle_WhenNoClients_ShouldReturnEmpty()
    {
        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetClientsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapAllDtoFieldsCorrectly()
    {
        var client = MakeClient(1);

        _repo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Client, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Client> { client });

        var handler = CreateHandler();
        var result  = (await handler.Handle(new GetClientsQuery(), CancellationToken.None)).First();

        result.ClientId.Should().Be(client.ClientId);
        result.TenantId.Should().Be(client.TenantId);
        result.ClientCode.Should().Be(client.ClientCode);
        result.ClientName.Should().Be(client.ClientName);
        result.Email.Should().Be(client.Email);
        result.Status.Should().Be(client.Status);
    }
}

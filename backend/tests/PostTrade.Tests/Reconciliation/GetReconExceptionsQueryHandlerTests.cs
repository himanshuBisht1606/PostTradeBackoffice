using PostTrade.Application.Features.Reconciliation.Queries;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.Reconciliation;
using PostTrade.Domain.Enums;

namespace PostTrade.Tests.Reconciliation;

public class GetReconExceptionsQueryHandlerTests
{
    private readonly Mock<IRepository<ReconException>> _repo          = new();
    private readonly Mock<ITenantContext>              _tenantContext = new();

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid ReconId  = Guid.NewGuid();

    private GetReconExceptionsQueryHandler CreateHandler()
    {
        _tenantContext.Setup(t => t.GetCurrentTenantId()).Returns(TenantId);
        return new GetReconExceptionsQueryHandler(_repo.Object, _tenantContext.Object);
    }

    private static ReconException MakeException(ExceptionStatus status = ExceptionStatus.Open) => new()
    {
        ExceptionId          = Guid.NewGuid(),
        ReconId              = ReconId,
        TenantId             = TenantId,
        ExceptionType        = ExceptionType.QuantityMismatch,
        ExceptionDescription = "Test exception",
        ReferenceNo          = "SN001",
        Amount               = 500m,
        Status               = status
    };

    [Fact]
    public async Task Handle_ShouldReturnAllExceptionsForTenant()
    {
        var exceptions = new List<ReconException> { MakeException(), MakeException() };

        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconException, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(exceptions);

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetReconExceptionsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenNoExceptions_ShouldReturnEmpty()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconException, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconException>());

        var handler = CreateHandler();
        var result  = await handler.Handle(new GetReconExceptionsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteringByReconId_ShouldPassPredicateToRepo()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconException, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconException>());

        var handler = CreateHandler();
        await handler.Handle(new GetReconExceptionsQuery(ReconId: ReconId), CancellationToken.None);

        _repo.Verify(r => r.FindAsync(It.IsAny<Expression<Func<ReconException, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldUseCurrentTenantId()
    {
        _repo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ReconException, bool>>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<ReconException>());

        var handler = CreateHandler();
        await handler.Handle(new GetReconExceptionsQuery(), CancellationToken.None);

        _tenantContext.Verify(t => t.GetCurrentTenantId(), Times.Once);
    }
}

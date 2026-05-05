using FluentValidation;
using MediatR;
using PostTrade.Application.Interfaces;
using PostTrade.Domain.Entities.PostTradeProcessing;

namespace PostTrade.Application.Features.PostTradeProcessing.FuturesAndOptions.Commands;

public record PostFoFinanceLedgerCommand(
    DateOnly TradeDate,
    string Exchange
) : IRequest<PostFoFinanceLedgerResultDto>;

public record PostFoFinanceLedgerResultDto(int ClientCount, string Message);

public class PostFoFinanceLedgerCommandValidator : AbstractValidator<PostFoFinanceLedgerCommand>
{
    public PostFoFinanceLedgerCommandValidator()
    {
        RuleFor(x => x.Exchange).NotEmpty().MaximumLength(10);
    }
}

public class PostFoFinanceLedgerCommandHandler : IRequestHandler<PostFoFinanceLedgerCommand, PostFoFinanceLedgerResultDto>
{
    private readonly IRepository<FoTradeBook> _tradeBookRepo;
    private readonly IRepository<FoSttLedger> _sttLedgerRepo;
    private readonly IRepository<FoStampDutyLedger> _stampDutyLedgerRepo;
    private readonly IRepository<FoClientPositionBook> _positionRepo;
    private readonly IRepository<FoFinanceLedger> _financeLedgerRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public PostFoFinanceLedgerCommandHandler(
        IRepository<FoTradeBook> tradeBookRepo,
        IRepository<FoSttLedger> sttLedgerRepo,
        IRepository<FoStampDutyLedger> stampDutyLedgerRepo,
        IRepository<FoClientPositionBook> positionRepo,
        IRepository<FoFinanceLedger> financeLedgerRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _tradeBookRepo = tradeBookRepo;
        _sttLedgerRepo = sttLedgerRepo;
        _stampDutyLedgerRepo = stampDutyLedgerRepo;
        _positionRepo = positionRepo;
        _financeLedgerRepo = financeLedgerRepo;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<PostFoFinanceLedgerResultDto> Handle(
        PostFoFinanceLedgerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.GetCurrentTenantId();

        // Prevent duplicate compute
        var existing = await _financeLedgerRepo.FirstOrDefaultAsync(
            f => f.TenantId == tenantId &&
                 f.TradeDate == request.TradeDate &&
                 f.Exchange == request.Exchange,
            cancellationToken);

        if (existing != null)
            throw new InvalidOperationException(
                $"Finance ledger already exists for {request.TradeDate:yyyy-MM-dd} / {request.Exchange}. " +
                "Delete it first to recompute.");

        // Load source data
        var trades = await _tradeBookRepo.FindAsync(
            t => t.TenantId == tenantId &&
                 t.TradeDate == request.TradeDate &&
                 t.Exchange == request.Exchange,
            cancellationToken);

        var sttRows = await _sttLedgerRepo.FindAsync(
            s => s.TenantId == tenantId &&
                 s.TradeDate == request.TradeDate &&
                 s.Exchange == request.Exchange,
            cancellationToken);

        var stampRows = await _stampDutyLedgerRepo.FindAsync(
            s => s.TenantId == tenantId &&
                 s.TradeDate == request.TradeDate &&
                 s.Exchange == request.Exchange,
            cancellationToken);

        var positions = await _positionRepo.FindAsync(
            p => p.TenantId == tenantId &&
                 p.TradeDate == request.TradeDate &&
                 p.Exchange == request.Exchange,
            cancellationToken);

        // Aggregate by client code
        var turnoverByClient = trades
            .GroupBy(t => t.ClientCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (
                    BuyTurnover: g.Sum(t => t.TradeValue * (t.Side == "B" ? 1 : 0)),
                    SellTurnover: g.Sum(t => t.TradeValue * (t.Side == "S" ? 1 : 0)),
                    ClearingMemberId: g.First().ClearingMemberId,
                    BrokerId: g.First().BrokerId,
                    ClientId: g.First().ClientId,
                    ClientName: g.First().ClientName
                ),
                StringComparer.OrdinalIgnoreCase);

        var sttByClient = sttRows
            .GroupBy(s => s.ClientCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (
                    TotalStt: g.Sum(s => s.TotalStt),
                    ClearingMemberId: g.First().ClearingMemberId,
                    BrokerId: g.First().BrokerId,
                    ClientId: g.First().ClientId,
                    ClientName: g.First().ClientName
                ),
                StringComparer.OrdinalIgnoreCase);

        var stampByClient = stampRows
            .GroupBy(s => s.ClientCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(s => s.TotalStampDuty),
                StringComparer.OrdinalIgnoreCase);

        var settlementByClient = positions
            .GroupBy(p => p.ClientCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => (
                    DailyMtm: g.Sum(p => p.DailyMtmSettlement),
                    NetPremium: g.Sum(p => p.NetPremium),
                    FinalSettlement: g.Sum(p => p.FuturesFinalSettlement),
                    ExerciseAssignment: g.Sum(p => p.ExerciseAssignmentValue),
                    ClearingMemberId: g.First().ClearingMemberId,
                    BrokerId: g.First().BrokerId,
                    ClientId: g.First().ClientId,
                    ClientName: g.First().ClientName
                ),
                StringComparer.OrdinalIgnoreCase);

        // Union of all known client codes
        var allClients = turnoverByClient.Keys
            .Union(sttByClient.Keys, StringComparer.OrdinalIgnoreCase)
            .Union(stampByClient.Keys, StringComparer.OrdinalIgnoreCase)
            .Union(settlementByClient.Keys, StringComparer.OrdinalIgnoreCase)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var ledgerRows = new List<FoFinanceLedger>(allClients.Count);

        foreach (var clientCode in allClients)
        {
            turnoverByClient.TryGetValue(clientCode, out var to);
            sttByClient.TryGetValue(clientCode, out var stt);
            stampByClient.TryGetValue(clientCode, out var stamp);
            settlementByClient.TryGetValue(clientCode, out var settle);

            var buyTurnover = to.BuyTurnover;
            var sellTurnover = to.SellTurnover;
            var totalStt = stt.TotalStt;
            var totalStamp = stamp;
            var totalCharges = totalStt + totalStamp;  // brokerage/exchange/SEBI/IPFT/GST = 0 until step 4

            var settlementNet = settle.DailyMtm + settle.NetPremium + settle.FinalSettlement + settle.ExerciseAssignment;
            var netAmount = settlementNet - totalCharges;

            // Resolve client metadata from whichever source has it
            var clientId = to.ClientId ?? stt.ClientId ?? settle.ClientId;
            var clientName = to.ClientName ?? stt.ClientName ?? settle.ClientName;
            var clearingMemberId = to.ClearingMemberId ?? stt.ClearingMemberId ?? settle.ClearingMemberId ?? string.Empty;
            var brokerId = to.BrokerId ?? stt.BrokerId ?? settle.BrokerId ?? string.Empty;

            ledgerRows.Add(new FoFinanceLedger
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                TradeDate = request.TradeDate,
                Exchange = request.Exchange,
                ClearingMemberId = clearingMemberId,
                BrokerId = brokerId,
                ClientCode = clientCode,
                ClientId = clientId,
                ClientName = clientName,
                BuyTurnover = buyTurnover,
                SellTurnover = sellTurnover,
                TotalTurnover = buyTurnover + sellTurnover,
                TotalStt = totalStt,
                TotalStampDuty = totalStamp,
                TotalCharges = totalCharges,
                DailyMtmSettlement = settle.DailyMtm,
                NetPremium = settle.NetPremium,
                FinalSettlement = settle.FinalSettlement,
                ExerciseAssignmentValue = settle.ExerciseAssignment,
                NetAmount = netAmount
            });
        }

        if (ledgerRows.Count > 0)
        {
            await _financeLedgerRepo.AddRangeAsync(ledgerRows, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new PostFoFinanceLedgerResultDto(
            ledgerRows.Count,
            $"Finance ledger computed for {ledgerRows.Count} client(s) on {request.TradeDate:yyyy-MM-dd} / {request.Exchange}.");
    }
}

namespace PostTrade.Application.Features.Reconciliation.DTOs;

public record ReconStatsDto(
    int TotalRecords,
    int Matched,
    int Mismatched,
    int Pending,
    int OpenExceptions,
    int ResolvedToday
);

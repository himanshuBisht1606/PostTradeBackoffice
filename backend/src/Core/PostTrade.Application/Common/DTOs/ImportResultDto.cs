namespace PostTrade.Application.Common.DTOs;

public record ImportResultDto(
    int Created,
    int Skipped,
    IEnumerable<ImportErrorDto> Errors
);

public record ImportErrorDto(int Row, string Reason);

using PostTrade.Domain.Enums;

namespace PostTrade.Application.Features.Reconciliation.DTOs;

public record ReconExceptionDto(
    Guid ExceptionId,
    Guid ReconId,
    Guid TenantId,
    ExceptionType ExceptionType,
    string ExceptionDescription,
    string ReferenceNo,
    decimal Amount,
    ExceptionStatus Status,
    string? Resolution,
    DateTime? ResolvedAt
);

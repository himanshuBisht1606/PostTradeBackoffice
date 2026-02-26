using MediatR;
using PostTrade.Application.Features.Auth.DTOs;

namespace PostTrade.Application.Features.Auth.Commands;

public record LoginCommand(
    string Username,
    string Password,
    string TenantCode
) : IRequest<LoginResponse>;

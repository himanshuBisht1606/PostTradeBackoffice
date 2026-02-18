# Post-Trade Backoffice — Pending Work

> **Reference doc** for future sessions. Read this at the start of each session to understand current status and what to build next.

---

## Project Overview

A broker-grade post-trade processing system built with:
- **Backend:** .NET 8, ASP.NET Core Minimal API, EF Core 8, PostgreSQL (Npgsql)
- **Architecture:** Clean Architecture — Domain → Application (CQRS/MediatR) → Infrastructure/Persistence → API
- **API style:** **Minimal API** (NOT controllers — `AddControllers` / `MapControllers` should be removed from Program.cs in Phase 3)
- **Auth:** JWT Bearer tokens with multi-tenancy (TenantId embedded in token)
- **Validation:** FluentValidation via MediatR pipeline behavior
- **Mapping:** AutoMapper

---

## Phase Status

| Phase | Description | Status |
|-------|-------------|--------|
| Phase 1 | Domain entities, DB schema, EF Core migrations | ✅ Done |
| Phase 2 | Core infrastructure (JWT, BCrypt, GenericRepo, MediatR pipeline, middleware) | ✅ Done |
| Phase 3 | Auth endpoints + cleanup | ✅ Done |
| Phase 4 | Master Setup endpoints | ✅ Done |
| Phase 5 | Trading endpoints | ✅ Done |
| Phase 6 | Settlement endpoints | ✅ Done |
| Phase 7 | Ledger endpoints | ⏳ Next |
| Phase 8 | Reconciliation endpoints | ⏳ Pending |
| Phase 9 | Corporate Actions endpoints | ⏳ Pending |
| Phase 10 | EOD Processing endpoints | ⏳ Pending |

---

## Phase 3 — Auth Module + Program.cs Cleanup (Next to build)

### 3a. Program.cs cleanup
- Remove `builder.Services.AddControllers()` and `app.MapControllers()`
- These belong to the controller pattern; we are using Minimal API

### 3b. Auth Feature — Application layer

**Location:** `src/Core/PostTrade.Application/Features/Auth/`

Files to create:
```
Features/Auth/
  Commands/
    LoginCommand.cs          — record: { Username, Password, TenantCode }
    LoginCommandHandler.cs   — validates user, verifies BCrypt password, returns JWT
    LoginCommandValidator.cs — FluentValidation rules
  DTOs/
    LoginResponse.cs         — { Token, ExpiresAt, Username, Roles }
```

Handler logic (`LoginCommandHandler`):
1. Find `Tenant` by `TenantCode` (must be Active)
2. Find `User` by `Username` within that tenant
3. Verify password via `PasswordService.VerifyPassword()`
4. Load user roles via `UserRole` → `Role`
5. Call `IJwtService.GenerateToken()` with userId, tenantId, username, roles
6. Return `LoginResponse`

Add AutoMapper mapping in `MappingProfile.cs`:
- No mapping needed for Auth (handler builds DTO manually)

### 3c. Auth Endpoints — API layer (Minimal API)

**Location:** `src/Presentation/PostTrade.API/Features/Auth/AuthEndpoints.cs`

```csharp
public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/login", async (LoginCommand cmd, ISender sender) =>
        {
            var result = await sender.Send(cmd);
            return Results.Ok(ApiResponse<LoginResponse>.Ok(result, "Login successful"));
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithOpenApi();

        return group;
    }
}
```

Register in `Program.cs`:
```csharp
app.MapGroup("/api/auth").MapAuthEndpoints();
```

---

## Phase 4 — Master Setup Module

Domain entities involved: `Tenant`, `Broker`, `Client`, `User`, `Role`, `Permission`, `UserRole`, `RolePermission`, `Exchange`, `Segment`, `Instrument`

### Endpoints (all Minimal API, all require Authorization except noted)

#### Tenants — `/api/tenants`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List all tenants |
| GET | `/{id}` | Get tenant by ID |
| POST | `/` | Create tenant |
| PUT | `/{id}` | Update tenant |
| DELETE | `/{id}` | Soft-delete tenant |

#### Brokers — `/api/brokers`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List brokers for current tenant |
| GET | `/{id}` | Get broker by ID |
| POST | `/` | Create broker |
| PUT | `/{id}` | Update broker |

#### Clients — `/api/clients`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List clients (with pagination) |
| GET | `/{id}` | Get client by ID |
| POST | `/` | Create client |
| PUT | `/{id}` | Update client |

#### Users — `/api/users`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List users |
| GET | `/{id}` | Get user by ID |
| POST | `/` | Create user (hash password with BCrypt) |
| PUT | `/{id}` | Update user |
| POST | `/{id}/roles` | Assign roles to user |

#### Roles & Permissions — `/api/roles`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List roles |
| POST | `/` | Create role |
| POST | `/{id}/permissions` | Assign permissions to role |

#### Exchanges — `/api/exchanges`
#### Segments — `/api/segments`
#### Instruments — `/api/instruments`
Standard CRUD for each.

### Application layer structure (per entity, example for Client):
```
Features/MasterSetup/Clients/
  Queries/
    GetClientsQuery.cs + Handler
    GetClientByIdQuery.cs + Handler
  Commands/
    CreateClientCommand.cs + Handler + Validator
    UpdateClientCommand.cs + Handler + Validator
  DTOs/
    ClientDto.cs
    CreateClientRequest.cs
    UpdateClientRequest.cs
Endpoints/
  ClientEndpoints.cs
```

---

## Phase 5 — Trading Module

Domain entities: `Trade`, `Position`, `PnLSnapshot`

#### Trades — `/api/trades`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List trades (filterable by date, client, instrument, status) |
| GET | `/{id}` | Get trade by ID |
| POST | `/` | Book a new trade |
| PUT | `/{id}/cancel` | Cancel a trade |
| GET | `/{id}/pnl` | Get P&L for a trade |

#### Positions — `/api/positions`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List open positions |
| GET | `/{clientId}` | Positions for a specific client |

#### P&L Snapshots — `/api/pnl`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List P&L snapshots |
| GET | `/{date}` | EOD P&L snapshot for a date |

---

## Phase 6 — Settlement Module

Domain entities: `SettlementBatch`, `SettlementObligation`

#### `/api/settlement/batches`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List settlement batches |
| POST | `/` | Create settlement batch |
| PUT | `/{id}/process` | Process a batch |

#### `/api/settlement/obligations`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List obligations |
| PUT | `/{id}/settle` | Mark obligation as settled |

---

## Phase 7 — Ledger Module

Domain entities: `LedgerEntry`, `ChargesConfiguration`

#### `/api/ledger`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/entries` | List ledger entries |
| POST | `/entries` | Post a ledger entry |
| GET | `/charges` | List charges config |
| POST | `/charges` | Create charges config |

---

## Phase 8 — Reconciliation Module

Domain entities: `Reconciliation`, `ReconException`

#### `/api/reconciliation`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List reconciliation records |
| POST | `/run` | Trigger reconciliation run |
| GET | `/exceptions` | List recon exceptions |
| PUT | `/exceptions/{id}/resolve` | Resolve an exception |

---

## Phase 9 — Corporate Actions Module

Domain entity: `CorporateAction`

#### `/api/corporate-actions`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List corporate actions |
| POST | `/` | Create corporate action |
| PUT | `/{id}/process` | Process a corporate action |

---

## Phase 10 — EOD Processing

Service: `EODProcessingService` (already has a stub in Infrastructure)

#### `/api/eod`
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/run` | Trigger EOD run for a date |
| GET | `/status/{date}` | Get EOD status for a date |

EOD run should:
1. Snapshot all open positions → `PnLSnapshot`
2. Generate settlement obligations
3. Run reconciliation
4. Close the trading day

---

## Minimal API Pattern (use this consistently)

### Endpoint file structure
Every module gets an `*Endpoints.cs` file under `src/Presentation/PostTrade.API/Features/{Module}/`:

```csharp
public static class TradeEndpoints
{
    public static RouteGroupBuilder MapTradeEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTradesQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<TradeDto>>.Ok(result));
        });

        group.MapPost("/", async (CreateTradeCommand cmd, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(cmd, ct);
            return Results.Created($"/api/trades/{result.Id}", ApiResponse<TradeDto>.Ok(result));
        })
        .WithName("CreateTrade");

        return group;
    }
}
```

### Program.cs endpoint registration pattern
```csharp
// All route groups registered here — in order of dependency
app.MapGroup("/api/auth").MapAuthEndpoints();
app.MapGroup("/api/tenants").MapTenantEndpoints().RequireAuthorization();
app.MapGroup("/api/brokers").MapBrokerEndpoints().RequireAuthorization();
app.MapGroup("/api/clients").MapClientEndpoints().RequireAuthorization();
app.MapGroup("/api/users").MapUserEndpoints().RequireAuthorization();
app.MapGroup("/api/roles").MapRoleEndpoints().RequireAuthorization();
app.MapGroup("/api/exchanges").MapExchangeEndpoints().RequireAuthorization();
app.MapGroup("/api/segments").MapSegmentEndpoints().RequireAuthorization();
app.MapGroup("/api/instruments").MapInstrumentEndpoints().RequireAuthorization();
app.MapGroup("/api/trades").MapTradeEndpoints().RequireAuthorization();
app.MapGroup("/api/positions").MapPositionEndpoints().RequireAuthorization();
app.MapGroup("/api/pnl").MapPnLEndpoints().RequireAuthorization();
app.MapGroup("/api/settlement").MapSettlementEndpoints().RequireAuthorization();
app.MapGroup("/api/ledger").MapLedgerEndpoints().RequireAuthorization();
app.MapGroup("/api/reconciliation").MapReconciliationEndpoints().RequireAuthorization();
app.MapGroup("/api/corporate-actions").MapCorporateActionEndpoints().RequireAuthorization();
app.MapGroup("/api/eod").MapEodEndpoints().RequireAuthorization();
```

### Response wrapper (already created in Phase 2)
```csharp
// Always use ApiResponse<T> for consistency
return Results.Ok(ApiResponse<T>.Ok(data, "optional message"));
return Results.BadRequest(ApiResponse<T>.Fail("message", errors));
```

---

## Key File Locations (Quick Reference)

| What | Where |
|------|-------|
| Domain entities | `src/Core/PostTrade.Domain/Entities/` |
| Application interfaces | `src/Core/PostTrade.Application/Interfaces/` |
| AutoMapper profile | `src/Core/PostTrade.Application/Common/Mappings/MappingProfile.cs` |
| Validation behavior | `src/Core/PostTrade.Application/Common/Behaviors/ValidationBehavior.cs` |
| JWT service | `src/Infrastructure/PostTrade.Infrastructure/Services/JwtService.cs` |
| Password service | `src/Infrastructure/PostTrade.Infrastructure/Services/PasswordService.cs` |
| Generic repository | `src/Infrastructure/PostTrade.Persistence/Repositories/GenericRepository.cs` |
| DB context | `src/Infrastructure/PostTrade.Persistence/Context/PostTradeDbContext.cs` |
| API response wrapper | `src/Presentation/PostTrade.API/Common/ApiResponse.cs` |
| Exception middleware | `src/Presentation/PostTrade.API/Middleware/ExceptionHandlingMiddleware.cs` |
| Tenant middleware | `src/Presentation/PostTrade.API/Middleware/TenantMiddleware.cs` |
| Program.cs | `src/Presentation/PostTrade.API/Program.cs` |
| appsettings.json | `src/Presentation/PostTrade.API/appsettings.json` |

---

## Notes & Decisions

- **Minimal API only** — do NOT use `[ApiController]` or inherit from `ControllerBase`
- All handlers follow the pattern: Command/Query → MediatR → Handler → returns DTO
- All validators use FluentValidation and are auto-discovered via `AddValidatorsFromAssembly`
- Multi-tenancy: every query/command handler that touches tenant-scoped data must call `ITenantContext.GetCurrentTenantId()`
- Soft deletes: prefer setting a `Status` enum to `Inactive`/`Deleted` rather than hard deleting rows
- JWT key in `appsettings.json` is a placeholder — replace with a real secret in production (use environment variables or secrets manager)
- `PasswordService` is a static class — no DI registration needed

# Post-Trade Backoffice — Developer Guide

> This document is the single reference for anyone new to the project.
> Read it top to bottom once. After that, use the section headers to navigate back to specific topics.

---

## Table of Contents

1. [What is this system?](#1-what-is-this-system)
2. [Tech Stack](#2-tech-stack)
3. [Architecture](#3-architecture)
4. [Project Structure](#4-project-structure)
5. [Domain Model](#5-domain-model)
6. [Request Lifecycle](#6-request-lifecycle)
7. [Authentication & Multi-Tenancy](#7-authentication--multi-tenancy)
8. [Modules & Endpoints](#8-modules--endpoints)
9. [Key Patterns](#9-key-patterns)
10. [Local Setup](#10-local-setup)
11. [Adding a New Feature](#11-adding-a-new-feature)

---

## 1. What is this system?

This is a **broker-grade post-trade processing backend**. It handles everything that happens after a trade is executed on an exchange:

- **Trade booking** — recording buy/sell trades with charges
- **Position management** — tracking net quantity and P&L per client/instrument
- **Settlement** — grouping trades into settlement batches, tracking obligations
- **Ledger** — double-entry style accounting entries per client
- **Reconciliation** — comparing system values against exchange values, flagging exceptions
- **Corporate actions** — dividends, bonuses, splits, rights
- **End-of-Day (EOD)** — snapshotting positions and P&L to close the trading day

The system is **multi-tenant** — multiple brokerages can run on the same deployment, with their data fully isolated.

---

## 2. Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 8 |
| API style | ASP.NET Core **Minimal API** (no controllers) |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL (via Npgsql) |
| Messaging | MediatR (CQRS pattern) |
| Validation | FluentValidation (wired as MediatR pipeline behavior) |
| Mapping | AutoMapper |
| Auth | JWT Bearer tokens (HMAC-SHA256) |
| Password hashing | BCrypt |
| API docs | Swashbuckle (OpenAPI spec) + Scalar UI |

---

## 3. Architecture

The project follows **Clean Architecture**. Dependencies always point inward — outer layers know about inner layers, never the reverse.

```
┌─────────────────────────────────────────────────────┐
│                  PostTrade.API                       │  ← Presentation layer
│   Minimal API endpoints, middleware, DI wiring       │
└────────────────────────┬────────────────────────────┘
                         │ depends on
┌────────────────────────▼────────────────────────────┐
│   PostTrade.Application (Core)                       │  ← Use-case layer
│   Commands, Queries, Handlers, DTOs, Interfaces      │
└────────────────────────┬────────────────────────────┘
                         │ depends on
┌────────────────────────▼────────────────────────────┐
│   PostTrade.Domain (Core)                            │  ← Domain layer
│   Entities, Enums, Domain exceptions                 │
└─────────────────────────────────────────────────────┘

Separate from the above (implement Application interfaces):

┌──────────────────────────────────────────────────────┐
│   PostTrade.Infrastructure                           │  ← Infrastructure layer
│   JwtService, PasswordService, EODProcessingService  │
├──────────────────────────────────────────────────────┤
│   PostTrade.Persistence                              │  ← Persistence layer
│   DbContext, GenericRepository, UnitOfWork,          │
│   EF Core configurations, Migrations                 │
└──────────────────────────────────────────────────────┘
```

### Why Clean Architecture?
- **Domain and Application layers have zero infrastructure dependencies** — they can be unit-tested with no database or HTTP.
- Swapping PostgreSQL for another database only affects Persistence. Swapping JWT for another auth mechanism only affects Infrastructure.

---

## 4. Project Structure

```
backend/
└── src/
    ├── Core/
    │   ├── PostTrade.Domain/
    │   │   ├── Entities/
    │   │   │   ├── MasterData/      Tenant, Broker, Client, User, Role, Permission,
    │   │   │   │                    UserRole, RolePermission, Exchange, Segment, Instrument
    │   │   │   ├── Trading/         Trade, Position, PnLSnapshot
    │   │   │   ├── Settlement/      SettlementBatch, SettlementObligation
    │   │   │   ├── Ledger/          LedgerEntry, ChargesConfiguration
    │   │   │   ├── Reconciliation/  Reconciliation, ReconException
    │   │   │   ├── CorporateActions/CorporateAction
    │   │   │   └── Audit/           AuditLog
    │   │   ├── Enums/               All enum definitions (TradeStatus, TradeSide, etc.)
    │   │   └── Exceptions/          DomainException
    │   │
    │   └── PostTrade.Application/
    │       ├── Common/
    │       │   ├── Behaviors/       ValidationBehavior (MediatR pipeline)
    │       │   └── Mappings/        MappingProfile (AutoMapper)
    │       ├── Interfaces/          IRepository<T>, IUnitOfWork, ITenantContext,
    │       │                        IJwtService, IPasswordService, IEODProcessingService
    │       └── Features/
    │           ├── Auth/            Commands, DTOs
    │           ├── MasterSetup/     Clients/, Users/, Roles/, Tenants/, ...
    │           ├── Trading/         Trades/, Positions/, PnL/
    │           ├── Settlement/      Batches/, Obligations/
    │           ├── Ledger/          Entries/, Charges/
    │           ├── Reconciliation/  Commands/, Queries/, DTOs/
    │           ├── CorporateActions/Commands/, Queries/, DTOs/
    │           └── EOD/             Commands/, Queries/, DTOs/
    │
    ├── Infrastructure/
    │   ├── PostTrade.Infrastructure/
    │   │   ├── Services/            JwtService, PasswordService, TenantContext
    │   │   └── EOD/                 EODProcessingService
    │   │
    │   └── PostTrade.Persistence/
    │       ├── Context/             PostTradeDbContext
    │       ├── Repositories/        GenericRepository<T>
    │       ├── Configurations/      EF Core IEntityTypeConfiguration per entity
    │       ├── Migrations/          EF Core migration files
    │       └── UnitOfWork.cs
    │
    └── Presentation/
        └── PostTrade.API/
            ├── Program.cs           DI registration + endpoint mapping
            ├── appsettings.json
            ├── Common/              ApiResponse<T>
            ├── Middleware/          ExceptionHandlingMiddleware, TenantMiddleware
            └── Features/
                ├── Auth/            AuthEndpoints.cs
                ├── MasterSetup/     TenantEndpoints, BrokerEndpoints, ...
                ├── Trading/         TradeEndpoints, PositionEndpoints, PnLEndpoints
                ├── Settlement/      SettlementEndpoints
                ├── Ledger/          LedgerEndpoints
                ├── Reconciliation/  ReconciliationEndpoints
                ├── CorporateActions/CorporateActionEndpoints
                └── EOD/             EodEndpoints
```

---

## 5. Domain Model

### Base classes

Every entity inherits from one of these:

```
BaseEntity
  CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
  IsDeleted, DeletedAt, DeletedBy

BaseAuditableEntity : BaseEntity
  Version, AuditTrail
```

### Entities by module

#### Master Setup

| Entity | Primary Key | Key Fields | Notes |
|--------|------------|------------|-------|
| `Tenant` | `TenantId` | TenantCode, TenantName, Status, LicenseKey | Root of multi-tenancy. Everything belongs to a tenant. |
| `Broker` | `BrokerId` | TenantId, BrokerCode, BrokerName, Status | A tenant can have multiple brokers. |
| `Client` | `ClientId` | TenantId, ClientCode, ClientType, PAN | End investor/trader. ClientType = Individual / Corporate. |
| `User` | `UserId` | TenantId, Username, PasswordHash, Status | System users (staff). Passwords stored as BCrypt hashes. |
| `Role` | `RoleId` | RoleName | e.g. Admin, Trader, Operations. |
| `Permission` | `PermissionId` | PermissionName | Granular access flags. |
| `UserRole` | composite | UserId, RoleId | Many-to-many between User and Role. |
| `RolePermission` | composite | RoleId, PermissionId | Many-to-many between Role and Permission. |
| `Exchange` | `ExchangeId` | ExchangeCode (NSE, BSE, MCX) | Market where instruments trade. |
| `Segment` | `SegmentId` | SegmentCode (EQ, FO, CD) | Segment of an exchange. |
| `Instrument` | `InstrumentId` | ISIN, Symbol, InstrumentType, ExchangeId | Securities — Equity, Futures, Options, etc. |

#### Trading

| Entity | Primary Key | Key Fields | Notes |
|--------|------------|------------|-------|
| `Trade` | `TradeId` | TenantId, ClientId, InstrumentId, TradeNo, Side, Quantity, Price, TradeDate, Status | Core trade record. Carries all regulatory charges (STT, GST, Brokerage, etc.). |
| `Position` | `PositionId` | TenantId, ClientId, InstrumentId, PositionDate, NetQuantity | Aggregated position per client per instrument per day. Updated as trades are booked. |
| `PnLSnapshot` | `PnLId` | TenantId, ClientId, InstrumentId, SnapshotDate | EOD P&L snapshot taken from Position. Immutable after creation. |

#### Settlement

| Entity | Primary Key | Key Fields | Notes |
|--------|------------|------------|-------|
| `SettlementBatch` | `BatchId` | TenantId, SettlementNo, TradeDate, SettlementDate, ExchangeId, Status | Groups trades by settlement cycle. |
| `SettlementObligation` | `ObligationId` | TenantId, ClientId, BatchId, FundsPayIn/Out, SecuritiesPayIn/Out, Status | Per-client funds and securities obligation within a batch. |

#### Ledger

| Entity | Primary Key | Key Fields | Notes |
|--------|------------|------------|-------|
| `LedgerEntry` | `LedgerId` | TenantId, ClientId, VoucherNo, PostingDate, LedgerType, EntryType, Debit, Credit, Balance | Running balance ledger per client. VoucherNo is unique. |
| `ChargesConfiguration` | `ChargesConfigId` | TenantId, BrokerId, ChargeType, CalculationType, Rate, EffectiveFrom | Configures how each charge type (brokerage, STT, GST, etc.) is computed. |

#### Reconciliation

| Entity | Primary Key | Key Fields | Notes |
|--------|------------|------------|-------|
| `Reconciliation` | `ReconId` | TenantId, ReconDate, SettlementNo, ReconType, SystemValue, ExchangeValue, Difference, Status | Compares system figures against exchange figures. |
| `ReconException` | `ExceptionId` | ReconId, TenantId, ExceptionType, Amount, Status, Resolution | Auto-created when a reconciliation is Mismatched. |

#### Corporate Actions

| Entity | Primary Key | Key Fields | Notes |
|--------|------------|------------|-------|
| `CorporateAction` | `CorporateActionId` | TenantId, InstrumentId, ActionType, AnnouncementDate, ExDate, RecordDate, Status | Dividend, Bonus, Split, Rights, Merger, Demerger. |

### Key Enums

```
TradeSide:       Buy, Sell
TradeStatus:     Pending, Confirmed, Settled, Cancelled, Rejected
TradeSource:     Manual, Exchange, API
SettlementStatus:Pending, Processing, Completed, Failed
ObligationStatus:Pending, Settled, PartiallySettled, Failed
LedgerType:      ClientLedger, BrokerLedger, CashLedger, SecuritiesLedger
EntryType:       Trade, Charges, Payment, Receipt, Adjustment, CorporateAction
ChargeType:      Brokerage, STT, GST, ExchangeTxn, SEBI, StampDuty
CalculationType: Percentage, Flat, Slab
ReconType:       Trade, Position, Obligation, Funds, Securities
ReconStatus:     Pending, Matched, Mismatched, Resolved
ExceptionType:   QuantityMismatch, PriceMismatch, MissingTrade, DuplicateTrade, Other
ExceptionStatus: Open, InProgress, Resolved, Closed
CorporateActionType:   Dividend, Bonus, Split, Rights, Merger, Demerger
CorporateActionStatus: Announced, Processing, Completed, Cancelled
```

---

## 6. Request Lifecycle

Every HTTP request flows through this exact sequence:

```
HTTP Request
     │
     ▼
ExceptionHandlingMiddleware   ← catches all unhandled exceptions, returns standard JSON error
     │
     ▼
CORS Middleware
     │
     ▼
Authentication Middleware     ← validates JWT, populates HttpContext.User
     │
     ▼
TenantMiddleware              ← reads TenantId claim from JWT, calls ITenantContext.SetTenantId()
     │
     ▼
Authorization Middleware      ← enforces RequireAuthorization()
     │
     ▼
Minimal API Endpoint          ← deserializes request body into Command/Query record
     │
     ▼
ISender.Send(command)         ← MediatR dispatches to handler
     │
     ▼
ValidationBehavior            ← runs FluentValidation; throws ValidationException if invalid
     │                           ExceptionHandlingMiddleware turns this into 400 Bad Request
     ▼
Command / Query Handler       ← business logic; reads TenantId from ITenantContext
     │
     ▼
IRepository<T>                ← GenericRepository executes EF Core query
     │
     ▼
PostTradeDbContext             ← EF Core generates SQL
     │
     ▼
PostgreSQL
```

### Standard response shape

All endpoints return `ApiResponse<T>`:

```json
// Success
{
  "success": true,
  "data": { ... },
  "message": "optional message",
  "errors": []
}

// Failure (validation, not found, etc.)
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": ["SettlementNo must not be empty.", "TotalTrades must be >= 0."]
}
```

---

## 7. Authentication & Multi-Tenancy

### Login flow

```
POST /api/auth/login
  Body: { username, password, tenantCode }

Handler:
  1. Find Tenant by TenantCode (must be Active)
  2. Find User by Username within that Tenant
  3. BCrypt.Verify(password, user.PasswordHash)
  4. Load user's roles via UserRole → Role
  5. JwtService.GenerateToken(userId, tenantId, username, roles)
  6. Return { token, expiresAt, username, roles }
```

### JWT structure

The token is signed with HMAC-SHA256. It contains these claims:

| Claim | Value |
|-------|-------|
| `UserId` | GUID of the user |
| `TenantId` | GUID of the tenant |
| `Username` | login username |
| `sub` | same as UserId |
| `jti` | unique token ID |
| `role` | one claim per role name |

Default expiry: **480 minutes** (8 hours), configurable in `appsettings.json`.

### Multi-tenancy

`TenantMiddleware` runs after authentication. It reads the `TenantId` claim and calls `ITenantContext.SetTenantId()`. Every handler that touches tenant-scoped data then calls:

```csharp
var tenantId = _tenantContext.GetCurrentTenantId();
```

and filters all queries by that `tenantId`. The DB context also applies global query filters for the most sensitive entities (Broker, Client, User, Trade, Position, SettlementBatch, LedgerEntry) so that EF Core automatically appends `WHERE TenantId = @tenantId` to every query.

**Important:** `ITenantContext` is registered as `Scoped`, which means it lives for exactly one HTTP request.

---

## 8. Modules & Endpoints

All endpoints except `/api/auth/login` require `Authorization: Bearer <token>`.

### Auth

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/auth/login` | Get a JWT token |

---

### Master Setup

#### Tenants `/api/tenants`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List all tenants |
| GET | `/{id}` | Get by ID |
| POST | `/` | Create tenant |
| PUT | `/{id}` | Update tenant |
| DELETE | `/{id}` | Soft-delete tenant |

#### Brokers `/api/brokers`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List brokers for current tenant |
| GET | `/{id}` | Get by ID |
| POST | `/` | Create broker |
| PUT | `/{id}` | Update broker |

#### Clients `/api/clients`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List clients (paginated) |
| GET | `/{id}` | Get by ID |
| POST | `/` | Create client |
| PUT | `/{id}` | Update client |

#### Users `/api/users`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List users |
| GET | `/{id}` | Get by ID |
| POST | `/` | Create user (password is BCrypt-hashed) |
| PUT | `/{id}` | Update user |
| POST | `/{id}/roles` | Assign roles to a user |

#### Roles `/api/roles`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List roles |
| POST | `/` | Create role |
| POST | `/{id}/permissions` | Assign permissions to a role |

#### Reference data
| Route | Standard CRUD |
|-------|--------------|
| `/api/exchanges` | List, Get, Create, Update |
| `/api/segments` | List, Get, Create, Update |
| `/api/instruments` | List, Get, Create, Update |

---

### Trading

#### Trades `/api/trades`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List trades — filterable by date, client, instrument, status |
| GET | `/{id}` | Get trade by ID |
| POST | `/` | Book a new trade (calculates all charges) |
| PUT | `/{id}/cancel` | Cancel a trade |
| GET | `/{id}/pnl` | P&L for a specific trade |

#### Positions `/api/positions`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | All open positions for the tenant |
| GET | `/{clientId}` | Positions for a specific client |

#### P&L `/api/pnl`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List P&L snapshots |
| GET | `/{date}` | EOD snapshot for a specific date |

---

### Settlement

#### Batches `/api/settlement/batches`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List batches (filterable by status) |
| POST | `/` | Create a settlement batch |
| PUT | `/{id}/process` | Process batch — transitions `Pending → Processing → Completed`, settles all pending obligations within it |

#### Obligations `/api/settlement/obligations`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List obligations (filterable by batchId, status) |
| PUT | `/{id}/settle` | Mark a single obligation as settled |

---

### Ledger

#### Entries `/api/ledger/entries`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List entries — filterable by clientId, fromDate, toDate, ledgerType, entryType |
| POST | `/` | Post a ledger entry — running balance auto-computed from previous entries for the same client + ledger type |

**Balance computation:** `newBalance = lastBalance + credit - debit`

#### Charges Config `/api/ledger/charges`
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | List charge configurations (filterable by chargeType, isActive) |
| POST | `/` | Create a charge configuration |

---

### Reconciliation

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/reconciliation` | List records — filterable by reconDate, reconType, status |
| POST | `/api/reconciliation/run` | Trigger a recon run — computes `|SystemValue - ExchangeValue|`; status is `Matched` if within tolerance, `Mismatched` otherwise. A `ReconException` is auto-created on mismatch. |
| GET | `/api/reconciliation/exceptions` | List exceptions (filterable by reconId, status) |
| PUT | `/api/reconciliation/exceptions/{id}/resolve` | Resolve an exception — sets status to `Resolved`, records the resolution text |

---

### Corporate Actions

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/corporate-actions` | List — filterable by instrumentId, actionType, status |
| POST | `/api/corporate-actions` | Create a corporate action (starts in `Announced` status) |
| PUT | `/api/corporate-actions/{id}/process` | Process — transitions `Announced → Processing → Completed` |

**Validator rules:** action-type-specific fields are required (e.g. `DividendAmount` required when ActionType = Dividend, `BonusRatio` required for Bonus, `RightsRatio` + `RightsPrice` required for Rights, etc.).

---

### EOD Processing

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/eod/run` | Trigger EOD for a trading date |
| GET | `/api/eod/status/{date}` | Check if EOD has been run — returns snapshot count and timestamp |

**EOD run logic:**
1. Guards against re-running EOD for the same date (checks existing `PnLSnapshot` records)
2. Fetches all open positions (`NetQuantity != 0`) for the tenant
3. Creates a `PnLSnapshot` for each position capturing RealizedPnL, UnrealizedPnL, TotalPnL, MarketPrice
4. Calls `EODProcessingService` for orchestration logging (ValidateTrades → RebuildPositions → CalculatePnL → ApplyCharges → GenerateLedgerEntries → GenerateSettlementObligations → RunReconciliation → GenerateReports)

---

## 9. Key Patterns

### CQRS with MediatR

Every feature is split into:
- **Commands** — write operations (create/update/process). Return a DTO.
- **Queries** — read operations. Return DTO or `IEnumerable<DTO>`.

All commands and queries are `record` types implementing `IRequest<TResponse>`. The handler is in the same file as the command/query.

```csharp
// Command
public record CreateFooCommand(string Name, decimal Value) : IRequest<FooDto>;

// Handler (same file)
public class CreateFooCommandHandler : IRequestHandler<CreateFooCommand, FooDto>
{
    public async Task<FooDto> Handle(CreateFooCommand request, CancellationToken ct) { ... }
}
```

### FluentValidation as pipeline behavior

Validators live in the same file as their command:

```csharp
public class CreateFooCommandValidator : AbstractValidator<CreateFooCommand>
{
    public CreateFooCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Value).GreaterThan(0);
    }
}
```

`ValidationBehavior` intercepts every MediatR request, runs all matching validators, and throws `ValidationException` if any rules fail. `ExceptionHandlingMiddleware` catches that and returns HTTP 400.

### Generic Repository

```csharp
IRepository<T>
  GetByIdAsync(Guid id)
  GetAllAsync()
  FindAsync(Expression<Func<T, bool>> predicate)   ← use this for filtered queries
  AddAsync(T entity)
  UpdateAsync(T entity)
  DeleteAsync(T entity)
```

Always inject `IRepository<YourEntity>`, not the concrete class. Use `FindAsync` for all filtered queries.

### Unit of Work

After calling `AddAsync` or `UpdateAsync`, nothing is persisted until:

```csharp
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

If you need atomic multi-entity operations, use:

```csharp
await _unitOfWork.BeginTransactionAsync();
// ... multiple repo operations ...
await _unitOfWork.CommitTransactionAsync();
// on error:
await _unitOfWork.RollbackTransactionAsync();
```

### DTO projection — no AutoMapper in handlers

DTOs are projected manually using a static `ToDto()` method on the query handler:

```csharp
internal static FooDto ToDto(Foo entity) => new(entity.FooId, entity.Name, entity.Value);
```

The command handler calls `QueryHandler.ToDto(entity)` after saving so there is a single source of truth for the mapping. AutoMapper is registered but currently not used — manual projection keeps things explicit.

### Soft deletes

Never hard-delete rows. Set a `Status` enum to `Inactive` or `Deleted` (or set `IsDeleted = true` on `BaseEntity`).

### Minimal API endpoint pattern

```csharp
public static class FooEndpoints
{
    public static RouteGroupBuilder MapFooEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (ISender sender, CancellationToken ct) => {
            var result = await sender.Send(new GetFoosQuery(), ct);
            return Results.Ok(ApiResponse<IEnumerable<FooDto>>.Ok(result));
        }).WithTags("Foo");

        group.MapPost("/", async (CreateFooCommand cmd, ISender sender, CancellationToken ct) => {
            var result = await sender.Send(cmd, ct);
            return Results.Created($"/api/foos/{result.FooId}", ApiResponse<FooDto>.Ok(result, "Created"));
        }).WithTags("Foo");

        return group;
    }
}
```

Register in `Program.cs`:

```csharp
app.MapGroup("/api/foos").MapFooEndpoints().RequireAuthorization();
```

---

## 10. Local Setup

### Prerequisites

- .NET 8 SDK
- PostgreSQL 14+ running locally
- (Optional) A PostgreSQL client like DBeaver or psql

### Steps

**1. Clone and navigate**
```bash
git clone <repo-url>
cd PostTradeBackoffice/backend
```

**2. Configure the database**

Edit `src/Presentation/PostTrade.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=PostTradeDb;Username=postgres;Password=root"
}
```

Change `Username` and `Password` to match your local PostgreSQL credentials.

**3. Configure JWT**

The default key in `appsettings.json` is a placeholder. For local dev it works; for production replace it with a proper secret (32+ characters) stored in environment variables or a secrets manager:

```json
"Jwt": {
  "Key": "your-256-bit-secret-key-here-minimum-32-chars!!",
  "Issuer": "PostTradeBackoffice",
  "Audience": "PostTradeBackoffice",
  "ExpiryMinutes": 480
}
```

**4. Apply migrations**
```bash
dotnet ef database update --project src/Infrastructure/PostTrade.Persistence \
  --startup-project src/Presentation/PostTrade.API
```

This creates the `PostTradeDb` database and all tables.

**5. Run the API**
```bash
dotnet run --project src/Presentation/PostTrade.API
```

**6. Open API docs**

Navigate to `http://localhost:<port>/scalar/v1` to see the interactive Scalar API explorer.

The raw OpenAPI JSON is at `http://localhost:<port>/swagger/v1/swagger.json`.

### Creating the first tenant and user

Since all endpoints are tenant-scoped, you need seed data before you can log in:

1. Connect to the database directly and insert a `Tenant` row with a known `TenantCode`.
2. Insert a `User` row with `PasswordHash` = `BCrypt.HashPassword("yourpassword")`.
   - You can generate the hash from a small C# snippet or use an online BCrypt tool.
3. Call `POST /api/auth/login` with `{ "username": "...", "password": "...", "tenantCode": "..." }`.
4. Use the returned token as `Authorization: Bearer <token>` for all subsequent requests.

---

## 11. Adding a New Feature

Follow this checklist for any new command or query. Use an existing module (e.g. Settlement) as a reference.

### Step 1 — Domain entity (if new)
Add the entity class to `PostTrade.Domain/Entities/<Module>/`.
Inherit from `BaseAuditableEntity` (or `BaseEntity` for read-heavy / snapshot entities).
Add new enums to `PostTrade.Domain/Enums/`.

### Step 2 — EF Core configuration
Add an `IEntityTypeConfiguration<YourEntity>` class to `PostTrade.Persistence/Configurations/<Module>/`.
Register the table name, primary key, column types, indexes there.
The `PostTradeDbContext` picks it up automatically via `ApplyConfigurationsFromAssembly`.

### Step 3 — Migration
```bash
dotnet ef migrations add <MigrationName> \
  --project src/Infrastructure/PostTrade.Persistence \
  --startup-project src/Presentation/PostTrade.API
```

### Step 4 — DTO
Add a `record` DTO to `PostTrade.Application/Features/<Module>/DTOs/`.

### Step 5 — Query handler
```
Features/<Module>/Queries/GetThingsQuery.cs
```
- Record implementing `IRequest<IEnumerable<ThingDto>>`
- Handler class implementing `IRequestHandler<...>`
- Inject `IRepository<Thing>` and `ITenantContext`
- Filter by `tenantId`, call `FindAsync`, project with `ToDto()`
- Expose `internal static ThingDto ToDto(Thing e)` for reuse by command handlers

### Step 6 — Command handler
```
Features/<Module>/Commands/CreateThingCommand.cs
```
- Record implementing `IRequest<ThingDto>`
- `AbstractValidator<CreateThingCommand>` in the same file
- Handler: build entity, `AddAsync`, `SaveChangesAsync`, return `ToDto(entity)`

### Step 7 — Endpoint
```
PostTrade.API/Features/<Module>/ThingEndpoints.cs
```
- Static class with `MapThingEndpoints(this RouteGroupBuilder group)` extension method
- Map each route, call `sender.Send(...)`, wrap result in `ApiResponse<T>`

### Step 8 — Register in Program.cs
```csharp
app.MapGroup("/api/things").MapThingEndpoints().RequireAuthorization();
```

### Checklist summary

- [ ] Domain entity created
- [ ] EF Core configuration added
- [ ] Migration created and applied
- [ ] DTO record defined
- [ ] Query handler written (with `ToDto` method)
- [ ] Command handler written (with validator in same file)
- [ ] Endpoint file written
- [ ] Registered in `Program.cs`
- [ ] Build passes: `dotnet build --nologo`

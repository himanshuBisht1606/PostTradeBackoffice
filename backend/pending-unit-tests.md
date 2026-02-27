# Post-Trade Backoffice — Pending Unit Tests

> Track progress across sessions. Read this at the start of each test session.

---

## Test Stack

| Tool | Purpose |
|------|---------|
| **xUnit** | Test framework (`[Fact]`, `[Theory]`, `[InlineData]`) |
| **Moq** | Mocking interfaces (`IRepository<T>`, `IUnitOfWork`, etc.) |
| **FluentAssertions** | Readable assertions (`.Should().Be()`, `.Should().ThrowAsync()`) |
| **GlobalUsings.cs** | `global using Xunit/Moq/FluentAssertions` — no per-file imports needed |

**Test project:** `backend/tests/PostTrade.Tests/PostTrade.Tests.csproj`

---

## Phase Status

| Phase | Scope | Status |
|-------|-------|--------|
| Phase 1 | Auth + Client (MasterSetup) | ✅ Done — 41 tests |
| Phase 2 | Remaining MasterSetup modules | ✅ Done — 126 tests |
| Phase 3 | Trading module | ✅ Done — 58 tests |
| Phase 4 | Settlement + Ledger | ✅ Done — 61 tests |
| Phase 5 | Reconciliation + Corporate Actions + EOD + Middleware | ✅ Done — 65 tests |

---

## Phase 1 — DONE (41 tests)

**Files created:**
```
tests/PostTrade.Tests/
  GlobalUsings.cs
  Auth/
    LoginCommandValidatorTests.cs      (6 tests)
    LoginCommandHandlerTests.cs        (8 tests)
  MasterSetup/Clients/
    CreateClientCommandValidatorTests.cs  (8 tests)
    CreateClientCommandHandlerTests.cs    (4 tests)
    UpdateClientCommandHandlerTests.cs    (4 tests)
    GetClientsQueryHandlerTests.cs        (5 tests)
    GetClientByIdQueryHandlerTests.cs     (3 tests)
```

**Patterns established:**
- Mock `IRepository<T>.FindAsync` with `It.IsAny<Expression<Func<T, bool>>>()` — returns fixed test data
- Build handler in `CreateHandler()` helper; override specific mocks in each test after calling it
- Use `with { }` syntax on command records to vary a single field per validator test

---

## Phase 2 — DONE (126 tests)

**Files created:**
```
tests/PostTrade.Tests/MasterSetup/
  Tenants/
    CreateTenantCommandHandlerTests.cs    (4 tests)
    CreateTenantCommandValidatorTests.cs  (6 tests)
    UpdateTenantCommandHandlerTests.cs    (4 tests)
    DeleteTenantCommandHandlerTests.cs    (4 tests)
    GetTenantsQueryHandlerTests.cs        (3 tests)
    GetTenantByIdQueryHandlerTests.cs     (2 tests)
  Brokers/
    CreateBrokerCommandHandlerTests.cs    (5 tests)
    CreateBrokerCommandValidatorTests.cs  (5 tests)
    UpdateBrokerCommandHandlerTests.cs    (4 tests)
    GetBrokersQueryHandlerTests.cs        (4 tests)
  Users/
    CreateUserCommandHandlerTests.cs      (7 tests)
    CreateUserCommandValidatorTests.cs    (8 tests)
    UpdateUserCommandHandlerTests.cs      (4 tests)
    AssignRolesCommandHandlerTests.cs     (5 tests)
    GetUsersQueryHandlerTests.cs          (5 tests)
  Roles/
    CreateRoleCommandHandlerTests.cs      (5 tests)
    AssignPermissionsCommandHandlerTests.cs (5 tests)
    GetRolesQueryHandlerTests.cs          (4 tests)
  Exchanges/
    CreateExchangeCommandHandlerTests.cs  (5 tests)
    CreateExchangeCommandValidatorTests.cs (4 tests)
    UpdateExchangeCommandHandlerTests.cs  (4 tests)
    GetExchangesQueryHandlerTests.cs      (3 tests)
  Instruments/
    CreateInstrumentCommandHandlerTests.cs  (5 tests)
    CreateInstrumentCommandValidatorTests.cs (8 tests)
    UpdateInstrumentCommandHandlerTests.cs  (4 tests)
    GetInstrumentsQueryHandlerTests.cs      (5 tests)
```

---

## Phase 2 — MasterSetup (Remaining Modules) — ARCHIVED

### Files to create

```
tests/PostTrade.Tests/MasterSetup/
  Tenants/
    CreateTenantCommandHandlerTests.cs
    UpdateTenantCommandHandlerTests.cs
    GetTenantsQueryHandlerTests.cs
    GetTenantByIdQueryHandlerTests.cs
    CreateTenantCommandValidatorTests.cs
  Brokers/
    CreateBrokerCommandHandlerTests.cs
    UpdateBrokerCommandHandlerTests.cs
    GetBrokersQueryHandlerTests.cs
    CreateBrokerCommandValidatorTests.cs
  Users/
    CreateUserCommandHandlerTests.cs      ← password must be BCrypt-hashed
    UpdateUserCommandHandlerTests.cs
    GetUsersQueryHandlerTests.cs
    AssignRolesCommandHandlerTests.cs
    CreateUserCommandValidatorTests.cs
  Roles/
    CreateRoleCommandHandlerTests.cs
    GetRolesQueryHandlerTests.cs
    AssignPermissionsCommandHandlerTests.cs
  Exchanges/
    CreateExchangeCommandHandlerTests.cs
    GetExchangesQueryHandlerTests.cs
  Instruments/
    CreateInstrumentCommandHandlerTests.cs
    GetInstrumentsQueryHandlerTests.cs
```

### Key test cases per module

**Users — special cases:**
- `Handle_ShouldHashPasswordWithBCrypt` — verify password is never stored in plain text
- `Handle_ShouldSetStatusToActive` — new users start Active
- `Handle_ShouldAssignTenantId`

**Roles:**
- `Handle_WhenRoleAlreadyExists_ShouldNotDuplicate` (if unique constraint check exists in handler)
- `Handle_ShouldAssignPermissionsToRole`

---

## Phase 3 — Trading Module

### Files to create

```
tests/PostTrade.Tests/Trading/
  Trades/
    BookTradeCommandHandlerTests.cs
    BookTradeCommandValidatorTests.cs
    CancelTradeCommandHandlerTests.cs
    CancelTradeCommandValidatorTests.cs
    GetTradesQueryHandlerTests.cs
    GetTradeByIdQueryHandlerTests.cs
    GetTradePnLQueryHandlerTests.cs
  Positions/
    GetPositionsQueryHandlerTests.cs
    GetPositionsByClientQueryHandlerTests.cs
  PnL/
    GetPnLSnapshotsQueryHandlerTests.cs
    GetPnLSnapshotByDateQueryHandlerTests.cs
```

### Key test cases

**BookTradeCommand:**
- `Handle_ShouldCalculateTradeValueAsQuantityTimesPrice`
- `Handle_ShouldSetStatusToPending`
- `Handle_ShouldGenerateTradeNo`
- `Handle_ShouldAssignTenantId`
- Validator: `Quantity <= 0 → fail`, `Price <= 0 → fail`, `SettlementNo empty → fail`

**CancelTradeCommand:**
- `Handle_WhenTradeNotFound_ShouldReturnNull`
- `Handle_WhenTradeIsAlreadySettled_ShouldThrowInvalidOperationException`
- `Handle_WhenTradeIsPending_ShouldCancelSuccessfully`
- `Handle_ShouldSetRejectionReason`

**GetTradesQuery:**
- `Handle_ShouldFilterByClientId`
- `Handle_ShouldFilterByDateRange`
- `Handle_ShouldFilterByStatus`
- `Handle_ShouldFilterByInstrumentId`

---

## Phase 4 — Settlement + Ledger

### Files to create

```
tests/PostTrade.Tests/Settlement/
  CreateSettlementBatchCommandHandlerTests.cs
  CreateSettlementBatchCommandValidatorTests.cs
  ProcessSettlementBatchCommandHandlerTests.cs
  SettleObligationCommandHandlerTests.cs
  GetSettlementBatchesQueryHandlerTests.cs
  GetSettlementObligationsQueryHandlerTests.cs

tests/PostTrade.Tests/Ledger/
  PostLedgerEntryCommandHandlerTests.cs
  PostLedgerEntryCommandValidatorTests.cs
  CreateChargesConfigCommandHandlerTests.cs
  CreateChargesConfigCommandValidatorTests.cs
  GetLedgerEntriesQueryHandlerTests.cs
  GetChargesConfigQueryHandlerTests.cs
```

### Key test cases

**ProcessSettlementBatch:**
- `Handle_WhenBatchNotFound_ShouldReturnNull`
- `Handle_WhenBatchAlreadyProcessed_ShouldThrowInvalidOperationException`
- `Handle_ShouldSettleAllPendingObligationsInBatch`
- `Handle_ShouldTransitionStatusToCompleted`

**PostLedgerEntry:**
- `Handle_ShouldComputeBalanceAsLastBalancePlusCreditMinusDebit`
- `Handle_WhenNoPreviousEntries_ShouldStartBalanceFromZero`
- `Handle_WhenDebitAndCreditBothZero_ValidatorShouldFail`
- `Handle_WhenDebitIsNegative_ValidatorShouldFail`

**CreateChargesConfig:**
- `Handle_ShouldSetIsActiveToTrue`
- Validator: `MaxAmount < MinAmount → fail`, `EffectiveTo before EffectiveFrom → fail`

---

## Phase 5 — Reconciliation + Corporate Actions + EOD + Middleware

### Files to create

```
tests/PostTrade.Tests/Reconciliation/
  RunReconciliationCommandHandlerTests.cs
  RunReconciliationCommandValidatorTests.cs
  ResolveReconExceptionCommandHandlerTests.cs
  GetReconciliationsQueryHandlerTests.cs
  GetReconExceptionsQueryHandlerTests.cs

tests/PostTrade.Tests/CorporateActions/
  CreateCorporateActionCommandHandlerTests.cs
  CreateCorporateActionCommandValidatorTests.cs
  ProcessCorporateActionCommandHandlerTests.cs
  GetCorporateActionsQueryHandlerTests.cs

tests/PostTrade.Tests/EOD/
  RunEodCommandHandlerTests.cs
  RunEodCommandValidatorTests.cs
  GetEodStatusQueryHandlerTests.cs

tests/PostTrade.Tests/Middleware/
  ExceptionHandlingMiddlewareTests.cs
  TenantMiddlewareTests.cs
```

### Key test cases

**RunReconciliation:**
- `Handle_WhenDifferenceWithinTolerance_ShouldSetStatusMatched`
- `Handle_WhenDifferenceExceedsTolerance_ShouldSetStatusMismatched`
- `Handle_WhenMismatched_ShouldCreateReconException`
- `Handle_WhenMatched_ShouldNotCreateReconException`

**ResolveReconException:**
- `Handle_WhenExceptionNotFound_ShouldReturnNull`
- `Handle_WhenAlreadyResolved_ShouldThrowInvalidOperationException`
- `Handle_ShouldSetResolutionText`
- `Handle_ShouldSetResolvedAt`

**ProcessCorporateAction:**
- `Handle_WhenNotInAnnouncedStatus_ShouldThrowInvalidOperationException`
- `Handle_ShouldTransitionToCompleted`
- `Handle_ShouldSetIsProcessedTrue`

**CreateCorporateAction validators:**
- `Validate_WhenDividendAction_AndNoDividendAmount_ShouldFail`
- `Validate_WhenBonusAction_AndNoBonusRatio_ShouldFail`
- `Validate_WhenExDateBeforeAnnouncementDate_ShouldFail`
- `Validate_WhenRecordDateBeforeExDate_ShouldFail`

**RunEodCommand:**
- `Handle_WhenAlreadyProcessedForDate_ShouldThrowInvalidOperationException`
- `Handle_ShouldCreateSnapshotForEachOpenPosition`
- `Handle_WhenNoOpenPositions_ShouldReturnZeroSnapshotted`
- `Handle_ShouldCallEodProcessingService`

**ExceptionHandlingMiddleware:**
- `Invoke_WhenValidationExceptionThrown_ShouldReturn400`
- `Invoke_WhenUnauthorizedAccessExceptionThrown_ShouldReturn401`
- `Invoke_WhenUnhandledExceptionThrown_ShouldReturn500`
- `Invoke_WhenNoException_ShouldPassThrough`

**TenantMiddleware:**
- `Invoke_WhenTenantIdClaimPresent_ShouldSetTenantContext`
- `Invoke_WhenNoTenantIdClaim_ShouldNotSetTenantContext`
- `Invoke_WhenTenantIdIsInvalidGuid_ShouldNotSetTenantContext`

---

## Notes for future sessions

- Always run `dotnet test tests/PostTrade.Tests/PostTrade.Tests.csproj --nologo` to verify before committing
- Follow the `CreateHandler()` pattern — set up common mocks in the helper, override specific ones per-test after calling it
- Validator tests: use `with { Field = value }` on command records to test one field at a time
- Handler tests: always verify both the return value AND the side effects (repo calls, unit of work)
- Keep test method names in the form: `Handle_WhenCondition_ShouldExpectedBehaviour`

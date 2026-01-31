# Database Migration Guide

## PostgreSQL Setup

1. Install PostgreSQL 15+
2. Create database:

```sql
CREATE DATABASE PostTradeDb;
CREATE USER posttrade WITH PASSWORD 'YourSecurePassword';
GRANT ALL PRIVILEGES ON DATABASE PostTradeDb TO posttrade;
```

## Run Migrations

```bash
cd backend/src/Presentation/PostTrade.API

# Create initial migration
dotnet ef migrations add InitialCreate --project ../../Infrastructure/PostTrade.Persistence

# Apply to database
dotnet ef database update --project ../../Infrastructure/PostTrade.Persistence

# Add new migration (when entities change)
dotnet ef migrations add YourMigrationName --project ../../Infrastructure/PostTrade.Persistence
dotnet ef database update --project ../../Infrastructure/PostTrade.Persistence
```

## Connection String

Update in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=PostTradeDb;Username=posttrade;Password=YourPassword"
  }
}
```

## Tables Created

This migration creates 40+ tables including:

**Master Data:**
- Tenants, Brokers, Clients
- Users, Roles, Permissions
- Instruments, Exchanges, Segments

**Trading:**
- Trades, Positions, PnLSnapshots

**Settlement:**
- SettlementBatches, SettlementObligations

**Ledger:**
- LedgerEntries, ChargesConfigurations

**Reconciliation:**
- Reconciliations, ReconExceptions

**Corporate Actions:**
- CorporateActions

**Audit:**
- AuditLogs

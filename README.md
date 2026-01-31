# Post-Trade Backoffice - Complete System

A comprehensive, production-ready broker-grade post-trade backoffice system.

## Features

✅ **13 Complete Modules**
- Dashboard
- Master Setup  
- User & Access Control
- Trade Processing
- Positions & PnL
- Settlement & Clearing
- Ledger & Finance
- Reports
- Reconciliation
- Corporate Actions
- Settings/Admin
- Audit & Logs
- Support/Operations

✅ **40+ Domain Entities**
✅ **PostgreSQL Database**
✅ **Entity Framework Core 8**
✅ **Clean Architecture + CQRS**
✅ **Multi-tenant by Design**
✅ **EOD Processing**
✅ **Complete Audit Trail**

## Quick Start

### Prerequisites
- .NET 8 SDK
- PostgreSQL 15+
- Node.js 18+ (for frontend)

### Backend Setup

```bash
cd backend

# Restore packages
dotnet restore

# Update connection string
# Edit: src/Presentation/PostTrade.API/appsettings.json

# Create database and run migrations
cd src/Presentation/PostTrade.API
dotnet ef migrations add InitialCreate --project ../../Infrastructure/PostTrade.Persistence
dotnet ef database update --project ../../Infrastructure/PostTrade.Persistence

# Run the API
dotnet run
```

API will be available at: https://localhost:5001/swagger

## Database Schema

The system includes 40+ tables organized into schemas:
- `master` - Master data (Tenants, Brokers, Clients, Instruments)
- `trading` - Trade processing and positions
- `settlement` - Settlement and clearing
- `ledger` - Ledger and finance
- `recon` - Reconciliation
- `corporate` - Corporate actions
- `audit` - Audit and logs

## Architecture

```
Domain (40+ Entities)
    ↓
Application (CQRS + MediatR)
    ↓
Infrastructure (Services + EOD)
    ↓
Persistence (EF Core + PostgreSQL)
    ↓
API (REST + OpenAPI)
```

## Development

See CONSTITUTION.md for architectural principles and rules.

## License

Proprietary - All Rights Reserved

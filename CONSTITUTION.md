# Post-Trade Backoffice Constitution v2.0

## Core Principles

### 1. Financial Accuracy & Auditability
- Use `decimal(18,4)` for ALL monetary values
- Complete audit trail for every transaction
- Reproducible calculations with version history
- Soft deletes only - no hard deletes on financial data
- UTC timestamps on all records

### 2. Multi-Tenancy & Data Isolation
- Complete tenant isolation at database level
- TenantId in all queries via global filters
- No cross-tenant data access possible
- Tenant-specific configurations

### 3. Immutability of Settled Data
- Settled trades CANNOT be modified
- Use compensating entries for corrections
- Settlement batches are immutable once completed
- Approval workflow for all corrections

### 4. Reconciliation First
- No EOD without successful reconciliation
- Three-way reconciliation: Internal → Exchange → Clearing
- Exception tracking and resolution
- Audit trail for all reconciliations

### 5. Performance & Scalability
- EOD completion within 30 minutes
- Support 1M+ trades per day
- Real-time position calculations
- Database partitioning for large tables
- Read replicas for reporting

### 6. Idempotency & Reliability
- Idempotent operations (duplicate prevention)
- Retry logic with exponential backoff
- Correlation IDs for request tracing
- Transactional consistency

## Technical Stack

**Database**: PostgreSQL 15+
**ORM**: Entity Framework Core 8
**Backend**: .NET 8
**Architecture**: Clean Architecture + CQRS + DDD
**API**: REST with OpenAPI 3.0

## Database Standards

### Table Naming
- PascalCase for table names
- Plural names (e.g., Trades, Positions)
- Prefix with module (e.g., Master_Tenants, Trade_Trades)

### Column Naming
- PascalCase for column names
- Descriptive names (avoid abbreviations)
- Standard suffixes: Id, Date, Time, Amount, Status

### Indexes
- Clustered index on primary key
- Non-clustered on TenantId + frequently queried fields
- Covering indexes for common queries
- Partitioning on TenantId and Date columns

### Constraints
- Foreign keys for referential integrity
- Check constraints for business rules
- Unique constraints where applicable
- Default values for audit fields

## Development Workflow

1. Write OpenAPI specification
2. Generate DTOs from spec
3. Implement domain entities
4. Create EF Core configurations
5. Write application logic (CQRS)
6. Implement API controllers
7. Write tests (Unit + Integration)
8. Run architecture compliance tests

## Deployment

- Database migrations versioned
- Blue-green deployment strategy
- Health checks on all services
- Monitoring and alerting

---

**Version**: 2.0  
**Last Updated**: 2025-01-31

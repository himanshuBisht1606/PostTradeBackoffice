# Post-Trade Backoffice Constitution v1.0

## Core Principles

### 1. Financial Accuracy & Auditability
- Use decimal for all monetary values (4+ decimal places)
- Complete audit trails for all transactions
- Reproducible calculations

### 2. Multi-Tenancy & Data Isolation
- Complete tenant isolation at all layers
- Validate tenant context on every query
- No cross-tenant data access

### 3. Immutability of Settled Data
- Settled trades cannot be modified
- Use compensating entries for corrections
- Maintain complete version history

### 4. Performance & Scalability
- Sub-30-minute EOD processing
- Support 1M+ trades per day
- Real-time position calculations

## Architecture

**Backend**: .NET 8 + Clean Architecture + CQRS
**Frontend**: React 18 + TypeScript
**Approach**: Specification-Driven Development

## Patterns

- CQRS for read/write separation
- Repository + Unit of Work
- Domain Events
- Event Sourcing for financial events
- OpenAPI-first development

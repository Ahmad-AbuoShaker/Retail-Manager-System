# Retail Manager System

ASP.NET Core 10 Web API

## Purpose

Backend system for managing products, sales, inventory, and users with role-based authentication. Server-computed prices and totals; clients send only ProductId and Quantity.

## Technology Stack

- **Framework**: .NET 10 (net10.0)
- **Architecture**: Clean Architecture (4 layers)
- **ORM**: Entity Framework Core 10 with SQL Server
- **Validation**: FluentValidation
- **Authentication**: JWT Bearer tokens with BCrypt password hashing
- **Testing**: xUnit

## Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB or full instance)
- Git (for cloning)

## Solution Structure

```
Retail-Manager-System.slnx
├─ Domain/           ← Entities, value objects, domain services (no infra deps)
├─ Application/      ← Services, DTOs, validators, interfaces
├─ Infrastructure/   ← Data access, identity, security, migrations
├─ WebApi/           ← ASP.NET Core API entry point, controllers, middleware
├─ Tests/Domain.Tests/ ← xUnit tests (Domain only)
└─ Excluded: RMData, RMDesktopUI (legacy)
```

## Architecture Overview

**Clean Architecture** with 4 layers:

1. **Domain** — Pure entities (User, Product, Sale, SaleDetail, Role, UserRole, Inventory), business rules, no infrastructure dependencies
2. **Application** — Application services (AuthService, ProductService, SaleService, UserService, InventoryService), DTOs, FluentValidation validators, interfaces
3. **Infrastructure** — EF Core DbContext, SQL Server migrations, JWT token service, BCrypt password service
4. **WebApi** — Presentation layer: controllers, middleware pipeline, Swagger/OpenAPI

### Key Patterns

- Constructor injection only, scoped lifetime
- async/await with CancellationToken everywhere
- FluentValidation manual invocation via `IValidator<T>.ValidateAsync`
- `BaseResponse<T>` standardized responses (Succeeded, Message, Errors, Data, ErrorCode)
- Atomic transactions for sale stock deduction (`BeginTransactionAsync` + `ExecuteUpdateAsync` with `WHERE QuantityInStock >= Quantity`)
- JWT authentication via `IJwtTokenService` and `IPasswordService` (BCrypt)
- Role-based authorization using plain strings ("Cashier", "Admin", "WarehouseWorker", "Manager")

## Database Configuration

- **Provider**: SQL Server
- **Connection string**: `DefaultConnection` (read from appsettings.json or user-secrets)
- **Migrations**: 2 migrations in `Infrastructure/Data/Migrations/`
  1. `20261007083249_InitialCreate` — Initial tables (Users, Sales, Products, etc.)
  2. `20261012233323_AddRolesAndUserRoles` — Roles + UserRoles with seed data (Cashier, Admin, WarehouseWorker, Manager)
- **Key conventions**:
  - `datetime2` columns always UTC, default `GETUTCDATE()`
  - `QuantityInStock` has `CHECK >= 0`
  - `Email` has unique index
  - `Role.Name` has unique index
  - Decimal precision: 18, 2
  - `Product.IsTaxable` controls tax application
  - Sale stores `UserId` (not CashierId)
  - No stored computed values — `SaleDetail` stores `UnitPrice` and line-level tax

### Entity Configurations (per entity)

Each entity has a dedicated configuration class enforcing:

- Key settings and value generation
- Required/MaxLength constraints
- Check constraints for non-negative values
- Unique indexes on Email and Role.Name
- Default values using `GETUTCDATE()`
- Relationships with `DeleteBehavior` (Restrict or Cascade)

### Tax Configuration

- Rate stored in `Tax:Rate` section of `appsettings.json` (e.g., `0.0875` = 8.75%)
- Validated to be between 0 and 1
- Applied per-product based on `Product.IsTaxable` flag

## Build and Run

```bash
dotnet build                  # build solution
dotnet test                 # run tests
dotnet ef migrations add <Name> --project Infrastructure --startup-project WebApi --output-dir Data/Migrations
dotnet ef database update --project Infrastructure --startup-project WebApi   # apply migrations
```

### Development

1. Clone the repository
2. Set up user secrets for the WebApi project:

   ```bash
   dotnet user-secrets init --project WebApi
   dotnet user-secrets set Jwt:Key <your-32-byte-key>
   dotnet user-secrets set Jwt:Issuer <your-issuer>
   dotnet user-secrets set Jwt:Audience <your-audience>
   # Optional: admin seed
   dotnet user-secrets set Seed:AdminEmail admin@example.com
   dotnet user-secrets set Seed:AdminPassword <strong-password>
   ```

3. Update `appsettings.json` if needed (Tax rate, etc.)
4. Run `dotnet ef database update --project Infrastructure --startup-project WebApi` to apply migrations
5. Run `dotnet run` (from WebApi project)

The app will be available at `https://localhost:<port>` (Swagger at `/swagger`).

## Authentication

**JWT Bearer token authentication** is configured globally.

### Setup

Required configuration (via user-secrets or `appsettings.Development.json`):

| Key | Description |
|-----|-------------|
| `Jwt:Key` | Secret key for signing (32+ bytes recommended) — **throws if missing** |
| `Jwt:Issuer` | Token issuer |
| `Jwt:Audience` | Token audience |

### Login / Register

- `POST /api/auth/login` — returns JWT access token and refresh token
- `POST /api/auth/register` — registers a new user (adds "Cashier" role by default)
- Passwords are hashed with BCrypt
- Login returns generic "Invalid credentials" message for timing-attack protection

### Authorization

- `[Authorize]` — global auth enabled
- `[Authorize(Roles = "Cashier")]` — SalesController
- `[Authorize(Roles = "Cashier" | "Admin")]` — ProductController
- `[Authorize(Roles = "Manager" | "Admin")]` — ReportsController GET
- `[Authorize(Roles = "WarehouseWorker" | "Admin")]` — InventoryController POST

> **Note**: Roles are hardcoded as plain strings; not DB-backed at this time.


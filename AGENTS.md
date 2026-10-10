# Retail Manager (ASP.NET Core 10, Clean Architecture)

## Structure (Retail-Manager-System.slnx)
- Domain/ — Entities (User, Product, Inventory, Sale, SaleDetail) + Services/TaxCalculator.cs (pure static function)
- Application/ — Services (AuthService, ProductService, SaleService, UserService), DTOs/ (Auth, Product, Sales, User), Validators/ (Auth, Sales), Enums/SaleErrorCode.cs
  - Interfaces/ — all interfaces live here (unified 2026-10-10): IAuthService, IProductService, IUserService, ICurrentUserService, ISaleService, Data/IAppDbContext, Security/IJwtTokenService, IPasswordService
- Infrastructure/ — Data/ (AppDbContext, Configuration/*, Migrations/), Identity/Security/ (JwtTokenService, PasswordService)
- WebApi/ — Controllers/ (Auth, Product, Sales, User), Services/CurrentUserService.cs, Program.cs
- Tests/Domain.Tests/ — Domain-only tests (folder Domin/Services/TaxCalculatorTests.cs — known typo in the folder name, not fixed)
- Excluded from work: RMData (old SQL project), RMDesktopUI (old WPF, .NET Framework 4.8)

## Commands
- Build: `dotnet build`
- Full non-incremental build (to see all warnings): `dotnet build --no-incremental`
- Tests: `dotnet test`
- New migration: `dotnet ef migrations add <Name> --project Infrastructure --startup-project WebApi --output-dir Data/Migrations`
- Apply migrations: `dotnet ef database update --project Infrastructure --startup-project WebApi`

## Architectural rules
- DTOs are always separate from entities
- async with CancellationToken everywhere
- Services are always Scoped; no user data in Singleton
- Constructor injection only
- Identity comes from JWT via ICurrentUserService; no direct HttpContext access inside Application
- Validation (FluentValidation) is invoked manually inside each service via IValidator<T>.ValidateAsync (same pattern as AuthService and SaleService) — no auto-validation, no filters
- The server computes all prices and totals; the client only sends ProductId and Quantity
- Sales endpoints require [Authorize(Roles = "Cashier")]
- Service errors are returned via BaseResponse<T> with an enum ErrorCode (e.g. SaleErrorCode); never leak a raw exception message to the client
- Login: generic "Invalid credentials" message, and a dummy BCrypt hash check when the user doesn't exist, to prevent timing leaks

## Database rules
- Keys: int for regular tables, Guid for User only
- datetime2 always UTC
- No stored computed values (SaleDetail stores UnitPrice and line-level tax, not a stored total)
- Sale stores UserId (not CashierId)
- Product.QuantityInStock: default 0, CHECK >= 0
- Inventory: purchase log only
- Stock deduction on sale MUST be atomic: ExecuteUpdateAsync with WHERE QuantityInStock >= Quantity inside an explicit transaction; 0 rows affected = insufficient stock. Read-modify-write is forbidden
- Constraints: MaxLength, CHECK >= 0 on all quantities and amounts, FKs, unique index on Email
- Tax rate lives in appsettings as a decimal fraction (e.g. 0.0875); Product.IsTaxable controls whether it applies

## Working rules (important)
- Build the smallest working version first
- Never trust a summary of your own work — always show raw output (full git diff, full dotnet build, actual SQL query results)
- Never change real code just to make a test pass; if they conflict, report it instead of changing the design
- Never touch an already-applied migration; never delete/modify migration files without explicit permission
- Never use `git add .` or `git add -A` — stage explicit paths only
- Show `git diff --cached` before every commit, and wait for an explicit "approved" before committing
- Code and comments in English only: short, clear, direct
- No secrets in code; use appsettings.Development.json or user-secrets
- Always write descriptive commit messages

## Known issues (do not fix unless explicitly asked)
- 9-11 nullable reference warnings (CS8618/CS8625) in Domain and Application DTOs
- Test folder named "Domin" instead of "Domain"
- No rate limiter enabled, no seed data
- UseCors without a fully registered AddCors — verify before relying on it
- No GET /api/sales/{id}, so POST /api/sales returns 200, not 201
- Role "Cashier"/"Customer" hardcoded as plain strings in multiple places (AuthService, [Authorize] attribute, entity default) — not a current priority; may move to a DB-backed roles table later

## Marker files (created intentionally, leave as-is)
- Application/InterfaceMark.cs
- Infrastructure/IAssemblyMaker.cs
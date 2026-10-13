# Retail Manager System - Architecture Documentation

**.NET 10.0 Clean Architecture solution for retail management.**

## 1. Solution Structure and Project Responsibilities

The solution `Retail-Manager-System.slnx` contains 5 projects (excluding legacy RMData/RMDesktopUI):

| Project | Location | Framework | Responsibility |
|---------|----------|-----------|----------------|
| **Domain** | `Domain/Domain.csproj` | net10.0 | Pure entities (User, Product, Sale, SaleDetail, Role, UserRole, Inventory), business rules, **no infrastructure dependencies**. Defines entity classes, enums (ErrorCode), and domain services (TaxCalculator). |
| **Application** | `Application/Application.csproj` | net10.0 | Application services, DTOs, FluentValidation validators, interfaces. Depends on Domain only. Services: `AuthService`, `ProductService`, `SaleService`, `UserService`, `InventoryService`. Validators: `RegisterRequestValidator`, `AuthenticationRequestValidator`, `CreateSaleRequestValidator`, `CreateSaleItemRequestValidator`. DTOs: `BaseResponse<T>`, auth DTOs, product DTOs, sales DTOs, inventory DTOs. |
| **Infrastructure** | `Infrastructure/Infrastructure.csproj` | net10.0 | Data access, identity, security implementation. Depends on Application. Implements `IAppDbContext`, `AppDbContext` (EF Core SQL Server), `JwtTokenService`, `PasswordService` (BCrypt), migrations. |
| **WebApi** | `WebApi/WebApi.csproj` | net10.0 (Sdk="Microsoft.NET.Sdk.Web") | ASP.NET Core API entry point. Depends on Application and Infrastructure. Contains controllers, middleware pipeline, Swagger/OpenAPI, JWTBearer configuration, CORS, seed data. |
| **Tests/Domain.Tests** | `Tests/Domain.Tests/Tests.csproj` | net10.0 | xUnit test project. **References Domain project only**. Contains `TaxCalculatorTests`. Note: folder named "Domin" (typo). |

**Dependency rule**: Projects depend only on the layer directly beneath them. No circular dependencies. WebApi depends on Application and Infrastructure; Application depends on Domain; Infrastructure depends on Application; Domain has no dependencies.

## 2. Dependency Direction (Clean Architecture Layers)

```
Domain  ←  Application  ←  Infrastructure  ←  WebApi
     (entities)     (services/DTOs)     (data/security)   (presentation/middleware)
```

- **Domain**: No incoming dependencies. Contains entities, value objects, enums, domain services.
- **Application**: Depends on Domain. Contains application services implementing interfaces from `Application.Interfaces`, DTOs, validators. Uses constructor injection.
- **Infrastructure**: Depends on Application. Implements Application interfaces (repositories, services). Uses Entity Framework Core, BCrypt, JWT. Configures EF Core conventions and migrations.
- **WebApi**: Depends on Application and Infrastructure. Configures DI, authentication, middleware, Swagger. Controllers use services from Application layer.

**No project references layers above its immediate dependency**. The `InterfaceMark.cs` file in Application marks project boundaries intentionally.

## 3. Request Lifecycle

### HTTP Request Flow (WebApi pipeline)

1. **Middleware pipeline** (`Program.cs:156-171`):
   - Development: `UseSwagger()`, `UseSwaggerUI()`
   - `UseCors("UniAPICORSPolicy")` — CORS policy configured in services
   - `UseAuthentication()` — JWT Bearer token validation
   - `UseAuthorization()` — role-based authorization
   - `MapControllers()` — routes to controller actions

2. **Controller action** (e.g., `SalesController.Create`):
   - Receives `CreateSaleRequest` DTO from body
   - Service injected via constructor injection (scoped lifetime)

3. **Application Service** (e.g., `SaleService.CreateAsync`):
   - Validates request via `IValidator<CreateSaleRequest>.ValidateAsync()` (FluentValidation manual invocation)
   - Retrieves current user ID from `ICurrentUserService.UserId` (JWT claims via `CurrentUserService`)
   - Fetches products from DB, computes subtotal/tax using `TaxSettings.Rate` and `TaxCalculator.CalculateTax()`
   - Begins explicit transaction via `_context.Database.BeginTransactionAsync()`
   - **Atomic stock deduction**: `ExecuteUpdateAsync` with `WHERE QuantityInStock >= Quantity` — read-modify-write forbidden
   - If 0 rows affected → `Insufficient stock` error, transaction rolled back
   - Adds `Sale` with `SaleDetails` to context
   - `SaveChangesAsync()` — commits transaction

4. **Response**: `BaseResponse<T>` with `Succeeded`, `Message`, `Errors`, `Data`, `ErrorCode`

## 4. Entity Relationships and Database Access

### Entity Model (from `.csproj` `TargetFramework` net10.0, confirmed in all project files)

| Entity | Key | Notable Properties |
|--------|-----|--------------------|
| **User** | `Id` (Guid) | `Email` (max 256, unique index), `PasswordHash`, `CreatedDate` (UTC, default GETUTCDATE()), `UserRoles` navigation |
| **Product** | `Id` (int) | `ProductName` (max 100), `RetailPrice`, `IsTaxable` (bool, default true), `QuantityInStock` (int, default 0, CHECK >= 0), `CreatedDate` |
| **Sale** | `Id` (int) | `UserId` (Guid FK → User), `SaleDate` (UTC), `SubTotal`, `Tax`, `Total` (all CHECK >= 0), `SaleDetails` navigation |
| **SaleDetail** | `Id` (int) | `SaleId` (int FK → Sale), `ProductId` (int FK → Product), `Quantity` (int, default 1, CHECK >= 0), `UnitPrice` (decimal, CHECK >= 0), `Tax` (decimal, default 0, CHECK >= 0) |
| **Role** | `Id` (Guid) | `Name` (max 50, unique index), `UserRoles` navigation |
| **UserRole** | Composite PK: `UserId` (Guid) + `RoleId` (Guid) | Junction table for many-to-many between User and Role |
| **Inventory** | `Id` (int) | `ProductId` (int FK → Product), `Quantity` (CHECK >= 0), `PurchasePrice` (CHECK >= 0), `PurchaseDate` (UTC, default GETUTCDATE()) |

### Database Access Approach

- **EF Core 10** (`Microsoft.EntityFrameworkCore 10.0.12` package version across all projects)
- `AppDbContext` (`Infrastructure.Data.AppDbContext`) implements `IAppDbContext`
- **Entity configurations**: Each entity has a dedicated configuration class enforcing:
  - Key settings and value generation
  - Required/MaxLength constraints
  - **Check constraints** for non-negative values (e.g., `CK_Products_QuantityInStock_NonNegative`)
  - **Unique indexes** on `Email` and `Role.Name`
  - Default values using `GETUTCDATE()` (UTC)
  - Relationships with `DeleteBehavior` (Restrict or Cascade)
- **Decimal precision**: 18, 2 (configured via `ConfigureConventions` in `AppDbContext.OnModelCreating`)
- **`ExecuteUpdateAsync`** for atomic stock deduction (SaleService: line 117) — avoids read-modify-write race conditions
- **No stored computed values**: `SaleDetail` stores `UnitPrice` and line-level tax, not stored totals

### Tax Configuration

- Rate stored in `Tax:Rate` section of `appsettings.json` (e.g., `0.0875` = 8.75%)
- Validated in `Program.cs:82` to be between 0 and 1
- Applied per-product based on `Product.IsTaxable` flag
- Calculated via `TaxCalculator.CalculateTax(lineSubtotal, rate)` (Domain static function)

## 5. Dependency Injection Registrations

### `WebApi/Program.cs` key registrations:

```csharp
// Database (line 33-41)
builder.Services.AddDbContext<IAppDbContext, AppDbContext>(options =>
{
    options.UseSqlServer(ConnectionString).LogTo(Console.WriteLine, ...);
    if (builder.Environment.IsDevelopment()) options.EnableSensitiveDataLogging();
});

// Scoped service registration (line 53-57)
// Scans assembly for classes ending in "Service" and registers as scoped interface
builder.Services.Scan(scan => scan.FromAssemblyOf<Application.IAssemblyMaker>()
    .AddClasses(classes => classes.Where(t => t.Name.EndsWith("Service")))
    .AsMatchingInterface().WithScopedLifetime());

// Validators from Application assembly (line 65)
builder.Services.AddValidatorsFromAssemblyContaining<Application.IAssemblyMaker>();

// Current user service (line 51)
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Options configuration for Tax (line 80-83)
builder.Services.AddOptions<TaxSettings>()
    .BindConfiguration(TaxSettings.SectionName)  // "Tax" from appsettings.json
    .Validate(s => s.Rate >= 0 && s.Rate <= 1, "Tax rate must be between 0 and 1.")
    .ValidateOnStart();

// JWT: throws if Key missing (line 68-72)
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT Key is missing.");

// Authentication (line 85-98)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Authorization (line 103)
builder.Services.AddAuthorization();

// Controllers + JSON options (line 105)
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Swagger (line 107-135)
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options => { ... /* Bearer security definition */ ... });

// Seed data: development-only admin user (line 175-225)
if (app.Environment.IsDevelopment())
{ using var scope = app.Services.CreateScope(); ... }
```

**Important**: `Jwt:Key` must be configured (user-secrets or appsettings) or the app throws on startup. No default/fallback key.

## 6. Authentication and Authorization Flow

### JWT Bearer Token Authentication (configured in `Program.cs:85-98`)

1. **Login** (`AuthController.LoginAsync`):
   - Validates credentials via `IPasswordService.VerifyPassword()` (BCrypt)
   - If user not found or password mismatch → returns `"Invalid credentials"` (generic message for timing-attack protection)
   - On success → generates JWT access token (`JwtTokenService.GenerateAccessToken()`) and refresh token
   - Token includes: `ClaimTypes.NameIdentifier` (user Id), `ClaimTypes.Email`, and `ClaimTypes.Role` for each user role

2. **Token validation** (middleware, per-request):
   - Validates `Issuer`, `Audience`, `Lifetime` (30 min expiry), `SigningKey`
   - Claims extracted via `CurrentUserService` from HTTP context

3. **Authorization** (controller actions):
   - `[Authorize]` — global auth enabled
   - `[Authorize(Roles = "Cashier")]` — SalesController POST
   - `[Authorize(Roles = "Cashier" | "Admin")]` — ProductController
   - `[Authorize(Roles = "Manager" | "Admin")]` — ReportsController, InventoryController GET
   - `[Authorize(Roles = "WarehouseWorker" | "Admin")]` — InventoryController POST
   - **Note**: Roles are hardcoded as plain strings ("Cashier", "Admin", "WarehouseWorker", "Manager"), not DB-backed at this time

4. **Register** (`AuthController.RegisterAsync`):
   - Adds "Cashier" role by default for new users
   - Creates `UserRole` junction entry

### User Seeding (Development only, `Program.cs:175-225`)

- Reads `Seed:AdminEmail` and `Seed:AdminPassword` from configuration
- If not exists, creates admin user with all roles
- If exists, fills in missing roles

## 7. Validation and Error-Handling Strategy

### Validation (FluentValidation, manual invocation)

- **Pattern**: Each service manually invokes `IValidator<T>.ValidateAsync()` (not auto-validation, not filters)
- **AuthService** (line 36-40, 94-98): Validates `RegisterRequest` and `AuthenticationRequest`, returns `BaseResponse` with validation errors
- **SaleService** (line 33-42): Validates `CreateSaleRequest`, returns validation errors in `BaseResponse`
- **Validator classes**:
  - `RegisterRequestValidator`: Email, password, confirm password, name fields
  - `AuthenticationRequestValidator`: Email, password
  - `CreateSaleRequestValidator`: Items collection validation
  - `CreateSaleItemRequestValidator`: ProductId > 0, Quantity > 0

### Error-Handling (`BaseResponse<T>`)

- **Standardized response** with properties: `Succeeded` (bool), `Message` (string), `Errors` (List<string>), `Data` (T), `ErrorCode` (ErrorCode enum)
- **Service errors**: Returned via `BaseResponse<T>` with appropriate `ErrorCode` — never leak raw exception messages to client
- **Login**: Generic "Invalid credentials" message (timing-attack protection, prevents user enumeration)
- **Unexpected errors**: Caught with `try/catch`, logged to `Console.Error`, returned as `ErrorCode.Unexpected` with generic message
- **Validation failures**: Return `Succeeded = false`, `Message = "Validation failed"`, `Errors` list from validator

### ErrorCode enum (`Application.Enums.ErrorCode.cs`)

- Values include: `Success`, `ValidationFailed`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Unexpected`

## 8. Transaction Management and Design Patterns

### Transaction Pattern (SaleService: line 110-141)

- **Explicit transaction** via `using var transaction = await _context.Database.BeginTransactionAsync(ct)`
- **Atomic stock deduction**: 
  1. `ExecuteUpdateAsync` with `WHERE QuantityInStock >= Quantity` — checks stock AND deducts in one SQL statement
  2. If `rows == 0` → insufficient stock, `transaction.RollbackAsync()` and return error
  3. If all items pass → `_context.Sales.Add(sale)`, `SaveChangesAsync()`, `transaction.CommitAsync()`
- **Read-modify-write forbidden** — must use `ExecuteUpdateAsync` with stock check

### Key Design Patterns Observed

1. **Repository pattern** via `IAppDbContext` — DbSets exposed, configurations applied via `ApplyConfigurationsFromAssembly`
2. **Unit of Work** — `SaveChangesAsync()` commits all changes in context
3. **Strategy pattern** — `TaxCalculator` static function (Domain layer, pure calculation, no EF dependency)
4. **BaseResponse<T>** — standardized response wrapper across all services
5. **Constructor injection only** — no property injection anywhere
6. **Scoped lifetime** — all services registered as `WithScopedLifetime()`
7. **async/await with CancellationToken** — everywhere in service methods

## 9. Current Architectural Issues and Potential Risks

| Issue | Description | Severity |
|-------|-------------|----------|
| **Hardcoded role strings** | "Cashier", "Admin", "WarehouseWorker", "Manager" as plain strings in multiple places (`AuthService`, `[Authorize]` attributes, entity defaults) — not DB-backed | Medium |
| **No rate limiter** | Code commented out in `Program.cs:140-149` (`//builder.Services.AddRateLimiter(...)`) | Medium |
| **CORS registration** | `UniAPICORSPolicy` applied middleware but verify full `AddCors` registration exists | Medium |
| **Nullable warnings** | 9 CS8618/CS8625 warnings in Domain and Application DTOs (known issue) | Low |
| **Test folder typo** | "Domin" instead of "Domain" — references work but naming is inconsistent | Low |
| **No GET /api/sales/{id}** | Only POST /api/sales exists (returns 200, not 201) — missing read-by-id endpoint | Low |
| **EF Core net10.0** | Project targets .NET 10.0; verify SDK compatibility with your environment | Medium |
| **No seed data by default** | Admin seed only when `Seed:AdminEmail`/`Seed:AdminPassword` set in Development | Low |
| **JWT key required** | `Jwt:Key` must be configured or app throws on startup — no default/fallback | High (operational) |
| **Password hashing** | BCrypt (`BCrypt.Net-Next 4.2.0`) used; verify compatibility with .NET 10 | Low |
| **No global exception handler** | Unhandled exceptions logged to Console.Error; client receives generic `BaseResponse` message only | Medium |

### Missing / TODO Items (not implemented)

- Rate limiter configuration
- Fully registered CORS with `AddCors`
- DB-backed roles table (roles currently hardcoded)
- GET `/api/sales/{id}` endpoint
- User enumeration protection beyond generic "Invalid credentials" message
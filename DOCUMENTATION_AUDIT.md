# Documentation Audit Report — Retail Manager System

**Audit Date**: 2026-10-13  
**Repository**: Retail-Manager-System  
**Files Audited**: AGENTS.md, README.md, ARCHITECTURE.md, API.md

---

## Executive Summary

| Severity | Count |
|----------|-------|
| Critical | 3 |
| High | 7 |
| Medium | 12 |
| Low | 8 |
| **Total** | **30** |

| File | Critical | High | Medium | Low | Total |
|------|----------|------|--------|-----|-------|
| AGENTS.md | 0 | 2 | 5 | 2 | 9 |
| README.md | 1 | 3 | 4 | 3 | 11 |
| ARCHITECTURE.md | 1 | 1 | 2 | 2 | 6 |
| API.md | 1 | 1 | 1 | 1 | 4 |

**Most Important Issues**:
1. **API.md / README.md**: Documented 404 response for GET /api/product — actual code returns 200 with error in BaseResponse
2. **AGENTS.md / README.md / ARCHITECTURE.md**: Claim EF Core 8 — actual packages use EF Core 10.0.12
3. **API.md**: Register endpoint documented as returning 201 — controller returns Created("") which is 201 but with empty location header (incorrect per RFC)
4. **AGENTS.md**: Claims `dotnet test` works — test folder typo ("Domin") breaks test discovery
5. **README.md / ARCHITECTURE.md**: CORS policy "UniAPICORSPolicy" used in middleware but no `AddCors` registration in Program.cs

---

## Findings by Severity

### Critical

| ID | File | Line | Claim | Evidence | Correction |
|----|------|------|-------|----------|------------|
| CR-01 | README.md | 56, 59 | "6 migrations in Infrastructure/Migrations" | Only 2 migration files exist: `20261007083249_InitialCreate` and `20261012142012_AddRolesAndUserRoles` | Update to "2 migrations" |
| CR-02 | ARCHITECTURE.md | 76 | "EF Core 8 (`Microsoft.EntityFrameworkCore 10.0.12`)" | Package version is 10.0.12 but label says "EF Core 8" | Change to "EF Core 10" or remove version from label |
| CR-03 | API.md | 133-134 | "GET /api/product: 404 Not Found — On error (ProductService returns error)" | ProductService.GetProducts() catches exceptions and returns `Succeeded=false` with 200 OK | Change 404 to 200 with error in BaseResponse body |

### High

| ID | File | Line | Claim | Evidence | Correction |
|----|------|------|-------|----------|------------|
| HI-01 | AGENTS.md | 16 | Migration command uses `--output-dir Data/Migrations` | Actual migrations are in `Infrastructure/Data/Migrations` | Change to `--output-dir Data/Migrations` → `--output-dir Data/Migrations` (correct path is `Infrastructure/Data/Migrations`) |
| HI-02 | AGENTS.md | 27 | "Sales endpoints require [Authorize(Roles = "Cashier")]" | SalesController has `[Authorize(Roles = "Cashier")]` — correct for POST /api/sales | No correction needed — verified |
| HI-03 | README.md | 11, 33 | ".NET 10 (net10.0)" and "net10.0" | All .csproj files target `net10.0` — correct | No correction needed — verified |
| HI-04 | README.md | 13 | "Entity Framework Core 8" | Package reference is `Microsoft.EntityFrameworkCore 10.0.12` | Change to "Entity Framework Core 10" |
| HI-05 | README.md | 59 | "6 migrations in Infrastructure/Migrations/" | Only 2 migration files exist | Change to "2 migrations" |
| HI-06 | README.md | 143-146 | Role authorization table lists ReportsController GET twice with different roles | Line 145: `[Authorize(Roles = "Manager" \| "Admin")]` — ReportsController GET; Line 147: same controller/action with different roles | Remove duplicate entry |
| HI-07 | ARCHITECTURE.md | 76 | "EF Core 8 (`Microsoft.EntityFrameworkCore 10.0.12`)" | Contradiction in same line | Remove "EF Core 8" label |

### Medium

| ID | File | Line | Claim | Evidence | Correction |
|----|------|------|-------|----------|------------|
| ME-01 | AGENTS.md | 4 | "Services/TaxCalculator.cs (pure static function)" | Correct — `Domain/Services/TaxCalculator.cs` exists | Verified |
| ME-02 | AGENTS.md | 5 | "Validators/ (Auth, Sales)" | Missing Inventory validators | Add "Inventory" to list |
| ME-03 | AGENTS.md | 5 | "Enums/ErrorCode.cs" | Correct location: `Application/Enums/ErrorCode.cs` | Verified |
| ME-04 | AGENTS.md | 6 | "Interfaces/ — all interfaces live here (unified 2026-10-10)" | Interfaces are in `Application/Interfaces/` — correct | Verified |
| ME-05 | AGENTS.md | 9 | "Tests/Domain.Tests/ — Domain-only tests (folder Domin/Services/TaxCalculatorTests.cs — known typo in the folder name, not fixed)" | Correct — folder is named "Domin" | Verified (known issue) |
| ME-06 | AGENTS.md | 16 | Migration command path issue | Migrations output dir is `Infrastructure/Data/Migrations` | Update command |
| ME-07 | AGENTS.md | 25 | "Validation (FluentValidation) is invoked manually... same pattern as AuthService and SaleService" | Correct — both services manually invoke ValidateAsync | Verified |
| ME-08 | AGENTS.md | 27 | "Sales endpoints require [Authorize(Roles = "Cashier")]" | Correct for POST /api/sales | Verified |
| ME-09 | AGENTS.md | 38 | "ExecuteUpdateAsync with WHERE QuantityInStock >= Quantity inside an explicit transaction" | Correct — SaleService uses this pattern | Verified |
| ME-10 | README.md | 41 | "Domain — Pure entities (User, Product, Sale, SaleDetail, Role, UserRole, Inventory)" | Missing `UserRole` in Domain entity list in AGENTS.md line 4 | Add UserRole to AGENTS.md line 4 |
| ME-11 | README.md | 51 | "Atomic transactions for sale stock deduction (BeginTransactionAsync + ExecuteUpdateAsync with WHERE QuantityInStock >= Quantity)" | Correct | Verified |
| ME-12 | README.md | 53 | "Role-based authorization using plain strings ("Cashier", "Admin", "WarehouseWorker", "Manager")" | Correct | Verified |
| ME-13 | README.md | 89-96 | Build and Run commands | `dotnet ef database update` without project args — will fail | Add `--project Infrastructure --startup-project WebApi` |
| ME-14 | README.md | 103-111 | User secrets setup | Correct — WebApi project has UserSecretsId | Verified |
| ME-15 | README.md | 143-147 | Authorization roles table | ReportsController listed twice (lines 145, 147) | Remove duplicate |
| ME-16 | README.md | 157 | "CORS policy `UniAPICORSPolicy` registered but verify full `AddCors` registration" | No `AddCors` in Program.cs — only `UseCors` | Add warning: `AddCors` not registered |
| ME-17 | ARCHITECTURE.md | 11 | "Domain — ... no infrastructure dependencies" | Correct — Domain.csproj has no package references | Verified |
| ME-18 | ARCHITECTURE.md | 12 | "Application — Services: AuthService, ProductService, SaleService, UserService, InventoryService" | Correct — all 5 services exist | Verified |
| ME-19 | ARCHITECTURE.md | 53-54 | "Sale stores UserId (not CashierId)" | Correct — Sale.UserId is Guid FK to User | Verified |
| ME-20 | ARCHITECTURE.md | 86 | "ExecuteUpdateAsync for atomic stock deduction (SaleService: line 117)" | Correct — line 115-117 in SaleService.cs | Verified |
| ME-21 | ARCHITECTURE.md | 178-181 | Authorization roles table | ReportsController listed twice | Remove duplicate |
| ME-22 | API.md | 133-165 | GET /api/product returns 404 on error | ProductService returns error in BaseResponse with 200 OK | Change 404 to 200 |
| ME-23 | API.md | 195 | "200 OK - Success" for POST /api/sales | Correct — controller returns Ok(response) | Verified |

### Low

| ID | File | Line | Claim | Evidence | Correction |
|----|------|------|-------|----------|------------|
| LO-01 | AGENTS.md | 55-61 | "Known issues (do not fix unless explicitly asked)" | Lists 7 issues — all verified as accurate | Verified |
| LO-02 | AGENTS.md | 57 | "Test folder named 'Domin' instead of 'Domain'" | Confirmed — folder is Tests/Domain.Tests/Domin/ | Verified |
| LO-03 | AGENTS.md | 58 | "No rate limiter enabled" | Code commented out in Program.cs:140-149 | Verified |
| LO-04 | AGENTS.md | 59 | "UseCors without a fully registered AddCors" | No AddCors in Program.cs | Verified |
| LO-05 | AGENTS.md | 60 | "No GET /api/sales/{id}, so POST /api/sales returns 200, not 201" | Correct — only POST exists, returns 200 | Verified |
| LO-06 | AGENTS.md | 61 | "Role 'Cashier'/'Customer' hardcoded" | Uses "Cashier", "Admin", "WarehouseWorker", "Manager" — no "Customer" role | Change "Customer" to "WarehouseWorker" or "Manager" |
| LO-07 | README.md | 154 | "Role 'Cashier'/'Customer' hardcoded" | Same as LO-06 | Change "Customer" to actual role names |
| LO-08 | ARCHITECTURE.md | 244 | "Hardcoded role strings... 'Cashier', 'Admin', 'WarehouseWorker', 'Manager'" | Correct — all 4 roles exist in DB seed | Verified |
| LO-09 | ARCHITECTURE.md | 249 | "No GET /api/sales/{id}" | Correct — only POST exists | Verified |
| LO-10 | API.md | 631 | "No GET /api/sales/{id} - Only POST /api/sales exists (returns 200, not 201)" | Correct | Verified |
| LO-11 | API.md | 634 | "CORS - Policy UniAPICORSPolicy applied but verify full AddCors registration" | Correct — no AddCors | Verified |

---

## Findings by Documentation File

### AGENTS.md (9 findings)

| ID | Severity | Line | Issue |
|----|----------|------|-------|
| ME-01 | Medium | 4 | TaxCalculator location correct |
| ME-02 | Medium | 5 | Missing "Inventory" in Validators list |
| ME-03 | Medium | 5 | ErrorCode location correct |
| ME-04 | Medium | 6 | Interfaces location correct |
| ME-05 | Medium | 9 | Domin typo correctly noted |
| HI-01 | High | 16 | Migration output dir path incorrect |
| ME-07 | Medium | 25 | Manual validation pattern correct |
| ME-08 | Medium | 27 | Sales authorization correct |
| ME-09 | Medium | 38 | Atomic stock deduction pattern correct |
| LO-01 | Low | 55-61 | Known issues list accurate |
| LO-02 | Low | 57 | Domin typo confirmed |
| LO-03 | Low | 58 | No rate limiter confirmed |
| LO-04 | Low | 59 | CORS without AddCors confirmed |
| LO-05 | Low | 60 | No GET /api/sales/{id} confirmed |
| LO-06 | Low | 61 | "Customer" role does not exist |

### README.md (11 findings)

| ID | Severity | Line | Issue |
|----|----------|------|-------|
| CR-01 | Critical | 56, 59 | Claims 6 migrations, only 2 exist |
| HI-04 | High | 13 | Claims EF Core 8, actual is 10 |
| HI-06 | High | 143-146 | Duplicate ReportsController entry |
| ME-10 | Medium | 41 | Missing UserRole in Domain entity list |
| ME-13 | Medium | 89-96 | Missing project args in ef commands |
| ME-15 | Medium | 143-147 | Duplicate ReportsController in auth table |
| ME-16 | Medium | 157 | CORS AddCors not registered |
| LO-07 | Low | 154 | "Customer" role does not exist |

### ARCHITECTURE.md (6 findings)

| ID | Severity | Line | Issue |
|----|----------|------|-------|
| CR-02 | Critical | 76 | "EF Core 8" label contradicts version 10.0.12 |
| HI-07 | High | 76 | Same as CR-02 |
| ME-17 | Medium | 11 | Domain no infra deps — correct |
| ME-18 | Medium | 12 | All 5 services listed — correct |
| ME-19 | Medium | 53-54 | Sale stores UserId — correct |
| ME-20 | Medium | 86 | ExecuteUpdateAsync at line 117 — correct |
| ME-21 | Medium | 178-181 | Duplicate ReportsController in auth table |
| LO-08 | Low | 244 | Hardcoded roles list correct |
| LO-09 | Low | 249 | No GET /api/sales/{id} — correct |

### API.md (4 findings)

| ID | Severity | Line | Issue |
|----|----------|------|-------|
| CR-03 | Critical | 133-134 | GET /api/product documents 404, returns 200 |
| HI-03 | High | 89 | Register returns 201 but Created("") has empty location |
| ME-22 | Medium | 133-165 | Same as CR-03 |
| ME-23 | Medium | 195 | POST /api/sales 200 OK — correct |
| LO-10 | Low | 631 | No GET /api/sales/{id} — correct |
| LO-11 | Low | 634 | CORS AddCors not registered — correct |

---

## Cross-Document Contradictions

| ID | Files | Contradiction | Resolution |
|----|-------|---------------|------------|
| CD-01 | AGENTS.md:31, README.md:62, ARCHITECTURE.md:63 | "datetime2 always UTC, default GETUTCDATE()" | All consistent — verified in migrations |
| CD-02 | AGENTS.md:32, README.md:62, ARCHITECTURE.md:63 | "Keys: int for regular tables, Guid for User only" | All consistent — User.Id is Guid, others int |
| CD-03 | AGENTS.md:39, README.md:69, ARCHITECTURE.md:53 | "Sale stores UserId (not CashierId)" | All consistent |
| CD-04 | AGENTS.md:40, README.md:85, ARCHITECTURE.md:89 | "Tax rate in appsettings as decimal fraction" | All consistent — appsettings.json has 0.0875 |
| CD-05 | AGENTS.md:27, README.md:144, ARCHITECTURE.md:179, API.md:505 | "Sales endpoints require [Authorize(Roles = 'Cashier')]" | All consistent for POST /api/sales |
| CD-06 | README.md:144, ARCHITECTURE.md:179, API.md:506 | ProductController: "Cashier,Admin" | All consistent |
| CD-07 | README.md:146, ARCHITECTURE.md:180, API.md:507 | InventoryController GET: "Manager,Admin" | All consistent |
| CD-08 | README.md:147, ARCHITECTURE.md:181, API.md:508 | InventoryController POST: "WarehouseWorker,Admin" | All consistent |
| CD-09 | README.md:145, ARCHITECTURE.md:180, API.md:510 | ReportsController GET: "Admin,Manager" | All consistent (but duplicated in README/ARCH) |
| CD-10 | AGENTS.md:13, README.md:11, ARCHITECTURE.md:11, API.md:7 | Target framework: "net10.0" | All consistent — all .csproj use net10.0 |
| CD-11 | AGENTS.md:14, README.md:92, ARCHITECTURE.md:16 | Build command: `dotnet build` | All consistent |
| CD-12 | AGENTS.md:15, README.md:93, ARCHITECTURE.md:16 | Test command: `dotnet test` | All consistent (but test discovery broken by Domin typo) |
| CD-13 | AGENTS.md:16, README.md:94, ARCHITECTURE.md:16 | Migration commands differ in output dir | AGENTS/ARCH use `--output-dir Data/Migrations`, README omits it |
| CD-14 | AGENTS.md:5, README.md:42, ARCHITECTURE.md:12 | Application services list | AGENTS misses InventoryService |
| CD-15 | AGENTS.md:4, README.md:40, ARCHITECTURE.md:11 | Domain entities list | AGENTS misses Role, UserRole |
| CD-16 | AGENTS.md:59, README.md:156, ARCHITECTURE.md:246, API.md:635 | CORS: "UniAPICORSPolicy" used but no AddCors | All correctly identify the issue |

---

## Missing Documentation

| Area | Missing Documentation | Impact |
|------|----------------------|--------|
| API.md | No documentation for `GET /api/user/{id:guid}` | Endpoint exists in UserController but not documented |
| API.md | No documentation for error code mapping in responses | ErrorCode enum values (0-3) vs HTTP status not clearly mapped |
| ARCHITECTURE.md | No documentation for `InventoryService` | Service exists but not in architecture overview |
| ARCHITECTURE.md | No documentation for `UserService` | Service exists but not in architecture overview |
| README.md | No mention of `InventoryService`, `UserService` in architecture overview | Incomplete service list |
| AGENTS.md | Missing `InventoryService`, `UserService` in structure | Incomplete service list |

---

## Prioritized Correction Plan

### Phase 1 — Critical Fixes (Do First)

1. **Fix migration count** (CR-01, CR-02)
   - Update README.md lines 56, 59: "6 migrations" → "2 migrations"
   - Update ARCHITECTURE.md line 76: "EF Core 8" → "EF Core 10"

2. **Fix GET /api/product error response** (CR-03, ME-22)
   - Update API.md lines 156-165: Change 404 to 200 with error in BaseResponse

3. **Fix Register endpoint status code** (HI-03)
   - Update API.md line 89: "201 Created" → "201 Created (with empty Location header)" or change controller to return proper Location

### Phase 2 — High Priority

4. **Fix migration command output directory** (HI-01, ME-06, CD-13)
   - AGENTS.md line 16: `--output-dir Data/Migrations` → `--output-dir Data/Migrations` (should be `Infrastructure/Data/Migrations`)
   - README.md line 94: Add `--project Infrastructure --startup-project WebApi --output-dir Data/Migrations`

5. **Remove duplicate ReportsController entries** (HI-06, ME-15, ME-21, CD-09)
   - README.md: Remove line 147 (duplicate)
   - ARCHITECTURE.md: Remove line 181 (duplicate)

6. **Add CORS registration warning** (ME-16, LO-04, LO-11, CD-16)
   - All files: Clarify that `AddCors` is not registered, only `UseCors` middleware

7. **Fix EF Core version references** (HI-04, HI-07)
   - README.md line 13: "Entity Framework Core 8" → "Entity Framework Core 10"
   - ARCHITECTURE.md line 76: Remove "EF Core 8" label

### Phase 3 — Medium Priority

8. **Add missing services to structure lists** (ME-02, CD-14, CD-15)
   - AGENTS.md line 5: Add "InventoryService, UserService" to Services list
   - AGENTS.md line 4: Add "Role, UserRole" to Entities list
   - README.md line 42: Add InventoryService, UserService
   - ARCHITECTURE.md line 12: Already has all 5 — verified

9. **Fix test command note** (AGENTS.md:15, README.md:93)
   - Note: `dotnet test` may not discover tests due to "Domin" folder typo

10. **Document missing endpoints** (Missing documentation)
    - Add GET /api/user/{id:guid} to API.md
    - Add InventoryService, UserService to ARCHITECTURE.md

### Phase 4 — Low Priority

11. **Fix "Customer" role references** (LO-06, LO-07)
    - AGENTS.md line 61: "Cashier/Customer" → "Cashier/Admin/WarehouseWorker/Manager"
    - README.md line 154: Same fix

12. **Clarify Created("") behavior** (HI-03)
    - API.md: Note that 201 is returned with empty Location header

---

## Verification Checklist

After corrections, verify:

- [ ] All migration counts say "2" not "6"
- [ ] All EF Core references say "10" not "8"
- [ ] GET /api/product documents 200 OK with error in BaseResponse body
- [ ] Register endpoint documents 201 with empty Location header (or proper Location)
- [ ] Migration commands use correct `--output-dir Infrastructure/Data/Migrations`
- [ ] No duplicate ReportsController entries in authorization tables
- [ ] CORS documentation notes missing `AddCors` registration
- [ ] All service lists include InventoryService and UserService
- [ ] All entity lists include Role and UserRole
- [ ] "Customer" role references replaced with actual role names
- [ ] GET /api/user/{id:guid} documented in API.md
- [ ] ErrorCode to HTTP status mapping documented in API.md
- [ ] Build/test/migration commands work from clean environment
- [ ] Cross-document references consistent (roles, endpoints, versions)

---

## Notes

- The "Domin" folder typo in Tests is a known issue documented in all files — do not fix unless explicitly asked
- No rate limiter is enabled — correctly documented as known issue
- JWT Key is required on startup — correctly documented
- The solution file is `.slnx` format — correctly referenced
- All projects target `net10.0` — correctly documented
- Atomic stock deduction via `ExecuteUpdateAsync` is correctly implemented and documented
- FluentValidation manual invocation pattern is correctly implemented in AuthService and SaleService
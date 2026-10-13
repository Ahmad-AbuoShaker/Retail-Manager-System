# API Documentation

## 1. API Overview

**Purpose**: Retail Management System API for managing products, sales, inventory, users, and reports with role-based access control.

**Base URL**: `https://localhost:{port}/api` (port determined by launch settings)

**API Versioning**: Not implemented. Single version at `/api` prefix.

**Swagger/OpenAPI**: Available at `/swagger` in Development environment only. Includes Bearer token security definition.

---

## 2. Authentication

### Login
- **Endpoint**: `POST /api/auth/login`
- **Purpose**: Authenticate user and obtain JWT access token
- **No authorization required**

**Request Body** (`AuthenticationRequest`):
```json
{
  "email": "string",
  "password": "string"
}
```

**Validation Rules** (`AuthenticationRequestValidator`):
- Email: Required, valid email format
- Password: Required

**Response** (`BaseResponse<AuthenticationResponse>`):
- **200 OK** - Success:
  ```json
  {
    "succeeded": true,
    "message": "Login successful",
    "data": {
      "accessToken": "eyJhbGciOiJIUzI1NiIs...",
      "refreshToken": null,  // Not yet implemented
      "roles": ["Cashier"]
    },
    "errors": [],
    "errorCode": null
  }
  ```
- **401 Unauthorized** - Invalid credentials:
  ```json
  {
    "succeeded": false,
    "message": "Invalid credentials",
    "data": null,
    "errors": ["Invalid credentials"],
    "errorCode": null
  }
  ```

**How to pass access token**: Include in Authorization header:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Register
- **Endpoint**: `POST /api/auth/register`
- **Purpose**: Register new user (automatically assigned "Cashier" role)
- **No authorization required**

**Request Body** (`RegisterRequest`):
```json
{
  "email": "user@example.com",
  "password": "string",
  "confirmPassword": "string",
  "firstName": "string",
  "lastName": "string"
}
```

**Validation Rules** (`RegisterRequestValidator`):
- Email: Required, valid email format, max 256 characters
- Password: Required, minimum 8 characters
- ConfirmPassword: Required, must match Password
- FirstName: Required, max 50 characters, no leading/trailing spaces
- LastName: Required, max 50 characters, no leading/trailing spaces

**Response** (`BaseResponse<AuthenticationResponse>`):
- **201 Created** - Success (returns 201 with empty location header):
  ```json
  {
    "succeeded": true,
    "message": "Registration successful",
    "data": {
      "accessToken": "eyJhbGciOiJIUzI1NiIs...",
      "refreshToken": null,  // Not yet implemented
      "roles": ["Cashier"]
    },
    "errors": [],
    "errorCode": null
  }
  ```
- **400 Bad Request** - Validation failed:
  ```json
  {
    "succeeded": false,
    "message": "Validation failed",
    "data": null,
    "errors": ["Email is required.", "Password must be at least 8 characters."],
    "errorCode": null
  }
  ```
- **409 Conflict** - Email already registered:
  ```json
  {
    "succeeded": false,
    "message": "Email already registered",
    "data": null,
    "errors": ["Email already registered"],
    "errorCode": null
  }
  ```

---

## 3. Endpoints by Resource

### 3.1 Products

#### Get All Products
- **Endpoint**: `GET /api/product`
- **Purpose**: Retrieve all products
- **Authorization**: `[Authorize(Roles = "Cashier,Admin")]`
- **Required Roles**: Cashier, Admin

**Response** (`BaseResponse<IEnumerable<ProductDto>>`):
- **200 OK** - Success:
  ```json
  {
    "succeeded": true,
    "message": "Products retrieved successfully",
    "data": [
      {
        "id": 1,
        "productName": "Product Name",
        "description": "Description",
        "quantityInStock": 100,
        "retailPrice": 29.99,
        "isTaxable": true
      }
    ],
    "errors": [],
    "errorCode": null
  }
  ```
- **404 Not Found** - On error (ProductService returns error):
  ```json
  {
    "succeeded": false,
    "message": "An error occurred while retrieving products.",
    "data": [],
    "errors": ["Error details"],
    "errorCode": null
  }
  ```

---

### 3.2 Sales

#### Create Sale
- **Endpoint**: `POST /api/sales`
- **Purpose**: Create a new sale with atomic stock deduction
- **Authorization**: `[Authorize(Roles = "Cashier")]`
- **Required Role**: Cashier

**Request Body** (`CreateSaleRequest`):
```json
{
  "items": [
    {
      "productId": 1,
      "quantity": 2
    }
  ]
}
```

**Validation Rules** (`CreateSaleRequestValidator`, `CreateSaleItemRequestValidator`):
- Items: Required, at least one item
- ProductId: Required, must be > 0
- Quantity: Required, must be > 0

**Response** (`BaseResponse<CreateSaleResponse>`):
- **200 OK** - Success:
  ```json
  {
    "succeeded": true,
    "message": "Sale created successfully.",
    "data": {
      "saleId": 1,
      "subtotal": 59.98,
      "tax": 5.25,
      "total": 65.23,
      "saleDate": "2026-10-12T10:30:00Z"
    },
    "errors": [],
    "errorCode": null
  }
  ```
- **400 Bad Request** - Validation failed:
  ```json
  {
    "succeeded": false,
    "message": "Validation failed",
    "data": null,
    "errors": ["At least one item is required.", "ProductId must be greater than 0."],
    "errorCode": null
  }
  ```
- **404 Not Found** - Product not found:
  ```json
  {
    "succeeded": false,
    "message": "Product 999 not found.",
    "data": null,
    "errors": ["Product 999 not found."],
    "errorCode": 1  // ErrorCode.NotFound
  }
  ```
- **409 Conflict** - Insufficient stock:
  ```json
  {
    "succeeded": false,
    "message": "Insufficient stock for product 1.",
    "data": null,
    "errors": ["Insufficient stock for product 1."],
    "errorCode": 2  // ErrorCode.Conflict
  }
  ```
- **500 Internal Server Error** - Unexpected error:
  ```json
  {
    "succeeded": false,
    "message": "An error occurred while creating the sale.",
    "data": null,
    "errors": [],
    "errorCode": 3  // ErrorCode.Unexpected
  }
  ```

**Note**: The server computes all prices and totals. Client only sends `ProductId` and `Quantity`. Stock deduction is atomic (ExecuteUpdateAsync with WHERE QuantityInStock >= Quantity).

---

### 3.3 Inventory

#### Get All Inventory
- **Endpoint**: `GET /api/inventory`
- **Purpose**: Retrieve all inventory records with product names
- **Authorization**: `[Authorize(Roles = "Manager,Admin")]`
- **Required Roles**: Manager, Admin

**Response** (`BaseResponse<IEnumerable<InventoryDto>>`):
- **200 OK** - Success:
  ```json
  {
    "succeeded": true,
    "message": "Inventory retrieved successfully",
    "data": [
      {
        "id": 1,
        "productId": 1,
        "productName": "Product Name",
        "quantity": 50,
        "purchasePrice": 15.00,
        "purchaseDate": "2026-10-10T10:00:00Z"
      }
    ],
    "errors": [],
    "errorCode": null
  }
  ```
- **404/409/500** - Error responses follow same pattern as Sales endpoints

#### Create Inventory (Purchase/Stock Increment)
- **Endpoint**: `POST /api/inventory`
- **Purpose**: Add inventory purchase record and increment product stock
- **Authorization**: `[Authorize(Roles = "WarehouseWorker,Admin")]`
- **Required Roles**: WarehouseWorker, Admin

**Request Body** (`CreateInventoryRequest`):
```json
{
  "productId": 1,
  "quantity": 50,
  "purchasePrice": 15.00,
  "purchaseDate": "2026-10-12T10:00:00Z"  // Optional, defaults to UTC now
}
```

**Validation Rules** (`CreateInventoryRequestValidator`):
- ProductId: Required, must be > 0
- Quantity: Required, must be > 0
- PurchasePrice: Required, must be >= 0
- PurchaseDate: Optional, if provided cannot be in the future (UTC)

**Response** (`BaseResponse<InventoryDto>`):
- **200 OK** - Success:
  ```json
  {
    "succeeded": true,
    "message": "Inventory created successfully.",
    "data": {
      "id": 1,
      "productId": 1,
      "productName": "Product Name",
      "quantity": 50,
      "purchasePrice": 15.00,
      "purchaseDate": "2026-10-12T10:00:00Z"
    },
    "errors": [],
    "errorCode": null
  }
  ```
- **400 Bad Request** - Validation failed:
  ```json
  {
    "succeeded": false,
    "message": "Validation failed",
    "data": null,
    "errors": ["ProductId must be greater than 0.", "PurchaseDate cannot be in the future."],
    "errorCode": null
  }
  ```
- **404 Not Found** - Product not found:
  ```json
  {
    "succeeded": false,
    "message": "Product not found",
    "data": null,
    "errors": ["Product not found"],
    "errorCode": 1  // ErrorCode.NotFound
  }
  ```
- **500 Internal Server Error** - Unexpected error

---

### 3.4 Users

#### Get Current User (Me)
- **Endpoint**: `GET /api/user/me`
- **Purpose**: Get authenticated user's profile
- **Authorization**: `[Authorize]` (any authenticated user)
- **Required**: Valid JWT token

**Response** (`BaseResponse<UserDto>`):
- **200 OK** - Success:
  ```json
  {
    "succeeded": true,
    "message": null,
    "data": {
      "id": "guid",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john@example.com",
      "roles": ["Cashier"],
      "createdDate": "2026-10-01T10:00:00Z"
    },
    "errors": [],
    "errorCode": null
  }
  ```
- **401 Unauthorized** - Invalid/missing token or user ID claim
- **404 Not Found** - User not found in database

#### Get User by ID
- **Endpoint**: `GET /api/user/{id:guid}`
- **Purpose**: Get user profile by ID
- **Authorization**: `[Authorize]` (any authenticated user)
- **Required**: Valid JWT token
- **Path Parameter**: `id` (Guid) - User ID

**Response** (`BaseResponse<UserDto>`):
- **200 OK** - Success (same structure as GetCurrentUser)
- **401 Unauthorized** - Invalid/missing token
- **404 Not Found** - User not found

---

### 3.5 Reports

#### Get Sales Report
- **Endpoint**: `GET /api/reports/sales`
- **Purpose**: Get sales report with cashier details
- **Authorization**: `[Authorize(Roles = "Admin,Manager")]`
- **Required Roles**: Admin, Manager

**Response** (`BaseResponse<IEnumerable<SalesReportDto>>`):
- **200 OK** - Success:
  ```json
  {
    "succeeded": true,
    "message": "Sales report retrieved successfully",
    "data": [
      {
        "saleDate": "2026-10-12T10:30:00Z",
        "subTotal": 59.98,
        "tax": 5.25,
        "total": 65.23,
        "cashierFirstName": "John",
        "cashierLastName": "Doe",
        "cashierEmail": "john@example.com"
      }
    ],
    "errors": [],
    "errorCode": null
  }
  ```
- **404/409/500** - Error responses follow same pattern as Sales endpoints

---

## 4. Response Format

### BaseResponse<T>
All API responses use the `BaseResponse<T>` wrapper:

```csharp
public class BaseResponse<T>
{
    public bool Succeeded { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; }
    public T Data { get; set; }
    public ErrorCode? ErrorCode { get; set; }
}
```

| Field | Type | Description |
|-------|------|-------------|
| `succeeded` | boolean | `true` for success, `false` for failure |
| `message` | string | Human-readable message |
| `errors` | string[] | List of error messages (empty on success) |
| `data` | T | Response payload (null on failure) |
| `errorCode` | integer? | `ErrorCode` enum value (null on success) |

### ErrorCode Enum
```csharp
public enum ErrorCode
{
    None = 0,
    NotFound = 1,
    Conflict = 2,
    Unexpected = 3
}
```

### Success Response Pattern
```json
{
  "succeeded": true,
  "message": "Operation description",
  "data": { ... },
  "errors": [],
  "errorCode": null
}
```

### Failure Response Pattern
```json
{
  "succeeded": false,
  "message": "Error description",
  "data": null,
  "errors": ["Detailed error 1", "Detailed error 2"],
  "errorCode": 1  // NotFound, Conflict, or Unexpected
}
```

### HTTP Status Code Mapping
| ErrorCode | HTTP Status |
|-----------|-------------|
| (success) | 200 OK (or 201 for Register) |
| ValidationFailed (not an enum, handled via 400) | 400 Bad Request |
| NotFound | 404 Not Found |
| Conflict | 409 Conflict |
| Unexpected | 500 Internal Server Error |
| Unauthorized (auth middleware) | 401 Unauthorized |
| Forbidden (auth middleware) | 403 Forbidden |

---

## 5. Authorization

### Role-Based Access Control

| Endpoint | Allowed Roles | Attribute |
|----------|---------------|-----------|
| `POST /api/auth/login` | None (public) | None |
| `POST /api/auth/register` | None (public) | None |
| `GET /api/product` | Cashier, Admin | `[Authorize(Roles = "Cashier,Admin")]` |
| `POST /api/sales` | Cashier | `[Authorize(Roles = "Cashier")]` |
| `GET /api/inventory` | Manager, Admin | `[Authorize(Roles = "Manager,Admin")]` |
| `POST /api/inventory` | WarehouseWorker, Admin | `[Authorize(Roles = "WarehouseWorker,Admin")]` |
| `GET /api/user/me` | Any authenticated | `[Authorize]` |
| `GET /api/user/{id}` | Any authenticated | `[Authorize]` |
| `GET /api/reports/sales` | Admin, Manager | `[Authorize(Roles = "Admin,Manager")]` |

### Token Requirements
- JWT Bearer token in `Authorization: Bearer <token>` header
- Token validated for: Issuer, Audience, Lifetime (30 min), SigningKey
- Claims used: `NameIdentifier` (UserId GUID), `Email`, `Role` (multiple)

### Role Assignment
- New users registered via `/api/auth/register` automatically receive "Cashier" role
- Admin seed user (development only) receives all roles: Cashier, Admin, WarehouseWorker, Manager
- Roles stored in `UserRoles` junction table, not as claims in token (roles looked up on login)

---

## 6. Practical Examples

### Register New User
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "cashier@example.com",
    "password": "Password123",
    "confirmPassword": "Password123",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

**Response** (201 Created):
```json
{
  "succeeded": true,
  "message": "Registration successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": null,  // Not yet implemented
    "roles": ["Cashier"]
  },
  "errors": [],
  "errorCode": null
}
```

### Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "cashier@example.com",
    "password": "Password123"
  }'
```

**Response** (200 OK):
```json
{
  "succeeded": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": null,  // Not yet implemented
    "roles": ["Cashier"]
  },
  "errors": [],
  "errorCode": null
}
```

### Get Products (Authenticated)
```bash
curl -X GET https://localhost:5001/api/product \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Create Sale (Cashier Only)
```bash
curl -X POST https://localhost:5001/api/sales \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      { "productId": 1, "quantity": 2 },
      { "productId": 2, "quantity": 1 }
    ]
  }'
```

### Get Inventory (Manager/Admin Only)
```bash
curl -X GET https://localhost:5001/api/inventory \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Create Inventory Record (WarehouseWorker/Admin Only)
```bash
curl -X POST https://localhost:5001/api/inventory \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 1,
    "quantity": 100,
    "purchasePrice": 10.50,
    "purchaseDate": "2026-10-12T10:00:00Z"
  }'
```

### Get Current User Profile
```bash
curl -X GET https://localhost:5001/api/user/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Get Sales Report (Admin/Manager Only)
```bash
curl -X GET https://localhost:5001/api/reports/sales \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Notes for Developers

1. **No GET /api/sales/{id}** - Only POST /api/sales exists (returns 200, not 201)
2. **Rate limiting** - Not enabled (code commented out in Program.cs)
3. **CORS** - Policy `UniAPICORSPolicy` applied but verify full `AddCors` registration
4. **Roles are hardcoded strings** - Not DB-backed; changes require code modification
5. **Tax calculation** - Server-side only; client never sends prices
6. **Stock deduction** - Atomic via `ExecuteUpdateAsync`; 0 rows = insufficient stock
7. **JWT Key required** - Must configure `Jwt:Key` in user-secrets or appsettings; app throws on startup if missing
8. **Swagger** - Only available in Development environment at `/swagger`
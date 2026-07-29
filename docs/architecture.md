# Bookify — System Architecture

## 1. Executive Summary

Bookify is a premium, multi-tenant appointment booking ecosystem connecting customers with service providers (Medical, Beauty, Wellness, Professional Services, Home Services) across 20+ countries. The system follows Clean Architecture principles with a .NET 10 backend, SQL Server persistence, and a Flutter cross-platform mobile client.

---

## 2. Architectural Principles

| Principle | Application |
|---|---|
| **Clean Architecture** | Domain layer has zero external dependencies; Infrastructure and Presentation depend inward |
| **CQRS** | Commands and Queries are separated via MediatR for write/read optimization |
| **SOLID** | Every class has a single responsibility; interfaces define contracts |
| **DRY** | Cross-cutting concerns (validation, logging, auth) are centralized |
| **Fail-Fast** | Validation happens at the boundary; invalid requests never reach domain logic |
| **Async All The Way** | No sync-over-async; every I/O operation is truly async |
| **Security by Design** | Auth, authorization, rate-limiting, and input sanitization are non-negotiable defaults |

---

## 3. High-Level Architecture

```
┌─────────────────────────────────────────────────┐
│              Flutter Mobile Client              │
│  (Riverpod · Dio · GoRouter · Hive · Freezed)   │
└───────────────────────┬─────────────────────────┘
                        │ HTTPS / JSON
                        ▼
┌─────────────────────────────────────────────────┐
│            API Gateway / Load Balancer           │
│           (Rate Limiting · Auth Proxy)           │
└───────────────────────┬─────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────┐
│           ASP.NET Core 10 Web API               │
│  ┌───────────┐ ┌─────────┐ ┌────────────────┐  │
│  │ Middleware │ │  CQRS   │ │  Controllers   │  │
│  │ (Auth,     │ │MediatR  │ │  (Minimal API  │  │
│  │  Logging,  │ │  Hand-  │ │   Versioned)   │  │
│  │  Exception)│ │  lers   │ │                │  │
│  └───────────┘ └─────────┘ └────────────────┘  │
└───────────────────────┬─────────────────────────┘
                        │
          ┌─────────────┼─────────────┐
          ▼             ▼             ▼
┌──────────────┐ ┌──────────┐ ┌──────────────┐
│   SQL Server │ │  Redis   │ │  Blob Store  │
│   (Primary)  │ │  (Cache) │ │  (Images)    │
└──────────────┘ └──────────┘ └──────────────┘
```

---

## 4. Solution Structure (Backend)

```
backend/
├── Bookify.sln
├── src/
│   ├── Bookify.Domain/               # Entities, ValueObjects, Aggregates, DomainEvents
│   ├── Bookify.Application/          # CQRS Commands/Queries, DTOs, Interfaces, Validators
│   ├── Bookify.Infrastructure/       # EF Core, Repositories, JWT, Email, FileStorage
│   └── Bookify.WebApi/               # Controllers, Middleware, Program.cs
├── tests/
│   ├── Bookify.Domain.Tests/
│   ├── Bookify.Application.Tests/
│   └── Bookify.WebApi.IntegrationTests/
└── Dockerfile
```

### 4.1 Bookify.Domain

Contains enterprise-wide business rules. No external dependencies.

**Key Namespaces:**
- `Entities/` — User, Business, Provider, Service, Appointment, Review, Notification, Availability
- `ValueObjects/` — Money, Address, PhoneNumber, Email, TimeRange, Rating, Currency, GeoLocation
- `Enums/` — AppointmentStatus, UserRole, BusinessCategory, BookingType, PaymentStatus
- `DomainEvents/` — AppointmentConfirmedEvent, ReviewSubmittedEvent, etc.
- `Exceptions/` — DomainException base class

### 4.2 Bookify.Application

Orchestrates use cases. Depends only on Domain.

**Key Namespaces:**
- `Commands/` — CreateAppointmentCommand, SubmitReviewCommand, etc.
- `Queries/` — SearchBusinessesQuery, GetProviderAvailabilityQuery, etc.
- `DTOs/` — BusinessProfileDto, AppointmentDto, SearchResultDto
- `Interfaces/` — IUserRepository, IBusinessRepository, IAppointmentRepository, INotificationService, IAISearchService, IPaymentService
- `Validators/` — FluentValidation validators paired 1:1 with commands
- `Behaviors/` — LoggingBehavior, ValidationBehavior, PerformanceBehavior
- `Mappings/` — AutoMapper/Mapster profiles
- `Common/` — PaginatedList, Result<T>, PagedQuery, SortQuery, SearchQuery

### 4.3 Bookify.Infrastructure

Implements interfaces from Application. Depends on Domain + Application.

**Key Namespaces:**
- `Persistence/` — AppDbContext, Migrations, EntityConfigurations, Repositories
- `Authentication/` — JwtService, RefreshTokenService, PasswordHasher
- `Services/` — NotificationService, PaymentService, FileStorageService, AISearchService
- `Cache/` — RedisCacheService, ICacheService abstraction
- `BackgroundJobs/` — AppointmentReminderJob, ExpiredTokenCleanupJob

### 4.4 Bookify.WebApi

Presentation layer. Configuration, middleware, controllers.

**Key Components:**
- `Controllers/` — (Versioned) AuthController, UsersController, BusinessesController, ServicesController, AppointmentsController, ReviewsController, NotificationsController, SettingsController, DashboardController, AdminController
- `Middleware/` — ExceptionHandlingMiddleware, RequestLoggingMiddleware, RateLimitingMiddleware
- `Filters/` — ValidationFilter, PerformanceFilter
- `Program.cs` — DI registration, Swagger config, Health checks, CORS, Serilog

---

## 5. Flutter Architecture (Mobile)

```
mobile/
├── lib/
│   ├── main.dart
│   ├── app.dart                      # MaterialApp.router with GoRouter
│   ├── core/
│   │   ├── theme/                    # LightTheme, DarkTheme, AmoledTheme, Typography
│   │   ├── constants/                # API URLs, storage keys
│   │   ├── errors/                   # Failure, Exceptions, Error handlers
│   │   ├── network/                  # Dio client, interceptors, retry logic
│   │   ├── storage/                  # Hive boxes, SecureStorage
│   │   ├── utils/                    # Date helpers, validators, formatters
│   │   └── extensions/               # BuildContext extensions, string extensions
│   ├── data/
│   │   ├── models/                   # JSON serializable models (from OpenAPI)
│   │   ├── repositories/             # Repository implementations
│   │   ├── datasources/              # Remote (API) and Local (Hive) data sources
│   │   └── providers/                # Riverpod providers for DI
│   ├── domain/
│   │   ├── entities/                 # Domain entities (freezed)
│   │   ├── repositories/             # Abstract repository interfaces
│   │   └── usecases/                 # Business logic use cases
│   ├── presentation/
│   │   ├── router/                   # GoRouter configuration, route guards
│   │   ├── shared/                   # Reusable widgets (GlassCard, SearchBar, BottomNav)
│   │   ├── onboarding/              # Screens + providers
│   │   ├── auth/                    # Login, Register, Biometric
│   │   ├── home/                    # Discovery, Categories, Featured
│   │   ├── search/                  # AI Search, Filters, Results
│   │   ├── business/                # Business/Provider profile
│   │   ├── booking/                 # Time slot selection, booking flow
│   │   ├── checkout/                # Payment methods, review
│   │   ├── confirmation/            # Digital ticket, QR code
│   │   ├── dashboard/               # Customer bookings, history
│   │   ├── chat/                    # Real-time messaging
│   │   ├── notifications/           # Notification list
│   │   └── settings/                # Profile, theme, privacy
│   └── l10n/                        # Localization ARB files
├── test/
├── pubspec.yaml
└── analysis_options.yaml
```

---

## 6. Cross-Cutting Concerns

### 6.1 Authentication & Authorization
- **JWT + Refresh Tokens** — Access token (15 min) + refresh token (7 days) rotation
- **Biometric Ready** — Flutter local_auth for fingerprint/FaceID
- **Role-Based** — Customer, Provider, BusinessOwner, Admin
- **Policy-Based** — Resource ownership checks via custom AuthorizationHandler

### 6.2 Validation
- **FluentValidation** on all command DTOs at the Application boundary
- **FluentValidation auto-validation** via MediatR pipeline behavior
- **Client-side validation** via form keys and Riverpod state

### 6.3 Logging
- **Serilog** with structured logging (JSON format)
- Elasticsearch sink for production; Console sink for development
- Request/response logging via middleware (excludes sensitive PII)

### 6.4 Caching
- **Redis** distributed cache for:
  - Business search results (TTL: 5 min)
  - Provider availability slots (TTL: 1 min)
  - Static data (categories, countries, currencies)
- In-memory cache for frequently-read configuration

### 6.5 Rate Limiting
- Fixed-window rate limiter via ASP.NET Core built-in middleware
- Anonymous: 100 req/min · Authenticated: 300 req/min · Premium: 1000 req/min

### 6.6 API Versioning
- URL path versioning: `/api/v1/appointments`
- Deprecation headers for sunset versions

### 6.7 Health Checks
- `/health` — Liveness probe
- `/health/ready` — Readiness probe (DB, Redis, Blob storage)
- `/health/startup` — Startup probe

### 6.8 Exception Handling
- Global `ExceptionHandlingMiddleware` returns RFC 7807 ProblemDetails
- Domain exceptions → 400/404 · Validation errors → 422 · Unauthorized → 401 · Forbidden → 403

---

## 7. Technology Stack

| Layer | Technology | Version |
|---|---|---|
| **Runtime** | .NET | 10.0 |
| **API Framework** | ASP.NET Core Minimal API + Controllers | 10.0 |
| **ORM** | Entity Framework Core | 10.0 |
| **Database** | SQL Server | 2022 |
| **Cache** | Redis | 7.x |
| **CQRS** | MediatR | 12.x |
| **Validation** | FluentValidation | 11.x |
| **Mapping** | Mapster | 7.x |
| **Auth** | JWT (System.IdentityModel.Tokens.Jwt) | 7.x |
| **Logging** | Serilog (Sinks: Console, Elasticsearch) | 4.x |
| **API Docs** | Swagger / OpenAPI (NSwag or Swashbuckle) | 7.x |
| **Background Jobs** | Quartz.NET or Hangfire | Latest |
| **Testing** | xUnit + FluentAssertions + NSubstitute | Latest |
| **Mobile Framework** | Flutter | Latest Stable |
| **State Management** | Riverpod + flutter_riverpod | 3.x |
| **HTTP Client** | Dio | 5.x |
| **Routing** | GoRouter | 14.x |
| **Local Storage** | Hive | 4.x |
| **Secure Storage** | flutter_secure_storage | Latest |
| **Code Gen** | Freezed + json_serializable | Latest |
| **Localization** | flutter_localizations + ARB | Latest |

---

## 8. Security Considerations

- **Password Hashing:** PBKDF2 (via ASP.NET Core Identity PasswordHasher)
- **JWT Signing:** HMAC-SHA256 with 256-bit key stored in User Secrets / Azure Key Vault
- **HTTPS:** Enforced at the load balancer level
- **CORS:** Whitelist specific origins
- **SQL Injection:** EF Core parameterized queries (no raw SQL)
- **XSS:** All HTML returned from API is sanitized; Flutter renders safely
- **CSRF:** Token-based auth (JWT) is immune to CSRF
- **Data Privacy:** GDPR-compliant; user data export/deletion endpoints

---

## 9. Performance Targets

| Metric | Target |
|---|---|
| API Response Time (p95) | < 200ms |
| Search Query Time | < 500ms |
| Booking Confirmation | < 1s |
| Concurrent Users | 10,000 |
| Uptime | 99.9% |

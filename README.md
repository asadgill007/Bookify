# Bookify 📅✨

**AI-Powered Global Multi-Service Appointment Booking Platform**

Bookify is a full-stack appointment booking platform that connects customers with service providers across any industry — from haircuts and salons to medical appointments and consulting. Built with **ASP.NET Core 10** backend and **Flutter** frontend, Bookify features AI-powered search, real-time availability, secure payments, and a rich provider management system.

---

## 🚀 Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 10, C# |
| **Frontend** | Flutter 3.44+ (Dart 3.12+) |
| **Database** | SQL Server (production), InMemory (development) |
| **Auth** | JWT Bearer Tokens with Refresh Token rotation |
| **API Docs** | Swagger / OpenAPI 3.0 |
| **Caching** | Redis (production), In-Memory (development) |
| **Background Jobs** | Hangfire (SQL Server storage) |
| **Payments** | Stripe integration |
| **Search** | AI-powered via configurable provider (OpenAI, Claude, Gemini) |
| **Maps** | OpenStreetMap (flutter_map) |
| **Push Notifications** | Firebase Cloud Messaging |
| **Logging** | Serilog (Console + File with rolling retention) |
| **Testing** | xUnit, FluentAssertions, Moq |

---

## 🏗️ Architecture

```
├── backend/
│   ├── src/
│   │   ├── Bookify.Domain/        # Entities, ValueObjects, Enums
│   │   ├── Bookify.Application/   # CQRS commands/queries, DTOs, interfaces
│   │   └── Bookify.WebApi/        # Controllers, middleware, Program.cs
│   └── tests/
│       ├── Bookify.Domain.Tests/
│       ├── Bookify.Application.Tests/
│       ├── Bookify.Infrastructure.Tests/
│       └── Bookify.WebApi.Tests/  # Integration + smoke tests
├── mobile/                        # Flutter cross-platform app
│   ├── lib/
│   │   ├── core/                  # API client, theme, router, services
│   │   └── features/              # Feature modules (auth, booking, etc.)
│   └── android/ios/web/           # Platform configs
└── docs/                          # Architecture, API spec, roadmap
```

### Key Design Decisions

- **CQRS with MediatR** — Clean separation of commands and queries
- **Clean Architecture** — Domain layer has zero external dependencies
- **Result Pattern** — All operations return `Result<T>` for consistent error handling
- **API Versioning** — URL + header versioning (`/api/v1/...`)
- **Rate Limiting** — 100 req/min standard, 10 req/min for auth endpoints
- **Health Checks** — Database connectivity monitoring with liveness/readiness
- **Security Headers** — CSP, X-Frame-Options, HSTS, and more via middleware

---

## 🔧 Setup Instructions

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Flutter 3.44+](https://docs.flutter.dev/get-started/install)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (optional — InMemory DB works for development)
- [Chrome](https://www.google.com/chrome/) (for web testing)

### Backend Setup

```bash
# 1. Navigate to backend
cd backend

# 2. Restore dependencies
dotnet restore

# 3. Build (should be 0 errors, 0 warnings)
dotnet build

# 4. Run tests (should be 117+ passing, 0 failing)
dotnet test

# 5. Configure secrets (do NOT commit real keys)
dotnet user-secrets set JwtKey "your-32-char-min-secret-key"

# 6. Run the API (uses InMemory DB by default in Development)
cd src/Bookify.WebApi
dotnet run

# API available at: http://localhost:5136
# Swagger UI at:    http://localhost:5136/swagger
```

### Environment Variables (Production)

Set these instead of modifying `appsettings.json`:

```bash
Jwt__Key                          # JWT signing key (min 32 chars)
ConnectionStrings__DefaultConnection  # SQL Server connection string
ConnectionStrings__Redis              # Redis connection string (optional)
ConnectionStrings__HangfireConnection # Hangfire SQL Server storage (optional)
Cors__AllowedOrigins__0               # https://yourdomain.com
Stripe__TestSecretKey                 # Stripe test secret key
Email__SmtpHost                       # SMTP server host
Email__SmtpUsername                   # SMTP username
Email__SmtpPassword                   # SMTP password
```

### Frontend (Flutter) Setup

```bash
# 1. Navigate to mobile
cd mobile

# 2. Get dependencies
flutter pub get

# 3. Run for web (Chrome)
flutter run -d chrome

# 4. Build for production
flutter build web --release        # Web
flutter build appbundle --release   # Android
flutter build ios --release         # iOS (requires macOS)
```

The API base URL can be set at build time:
```bash
flutter run -d chrome --dart-define=API_BASE_URL=http://localhost:5136
```

---

## 🧪 Running Tests

```bash
# Backend tests
cd backend
dotnet test

# Frontend analysis
cd mobile
flutter analyze

# Frontend tests
flutter test
```

### End-to-End Verification

The full provider journey is covered by an automated script against a running API:

```bash
# 1. Start the API (Development uses the InMemory DB + auto-seed)
cd backend/src/Bookify.WebApi && dotnet run
# 2. In another terminal, run the E2E journey:
node scripts/e2e_test.js
```

It verifies: seeded categories/businesses intact → register new business owner → onboard business (category, hours, service, provider) → confirm **Pending** and hidden from customer search → admin approves → confirm now visible in search → customer books an appointment against the new business.

### Test Results (Phase 1)

| Project | Passed | Failed | Skipped |
|---------|--------|--------|---------|
| Bookify.Domain.Tests | 58 | 0 | 0 |
| Bookify.Application.Tests | 24 | 0 | 0 |
| Bookify.Infrastructure.Tests | 35 | 0 | 0 |
| Bookify.WebApi.Tests | 7 | 0 | 8* |
| **Total** | **124** | **0** | **8** |

*Skipped tests are integration tests that require a running API instance.

---

## 📡 API Endpoints

| Area | Endpoints | Auth |
|------|-----------|------|
| **Health** | `GET /health` | Public |
| **Auth** | Register, Login, Refresh, Logout, Forgot/Reset Password | Mixed |
| **Users** | Profile CRUD, Password Change, Biometric Toggle | JWT |
| **Businesses** | Search (verified-only), Detail by Slug, Create, Update, My Businesses, Set Hours, Resubmit | Mixed |
| **Categories** | List all categories | Public |
| **Services** | Business services CRUD (`/businesses/{id}/services`) | JWT (owner) |
| **Providers** | Add staff to a business (`/businesses/{id}/providers`), availability, slots | Mixed |
| **Business Hours** | Weekly opening hours per business (`PUT /businesses/{id}/hours`) | JWT (owner) |
| **Business Verification** | Pending → Approved / Rejected lifecycle with reason; Admin review queue | Admin |
| **Appointments** | CRUD, Confirm, Cancel, Complete, Reschedule | JWT |
| **Providers** | Available Slots, Availability, Overrides | Mixed |
| **Reviews** | CRUD, Reply, Vote, Report, Statistics | Mixed |
| **Payments** | Initialize, Confirm, History | JWT |
| **Notifications** | List, Mark Read, Delete | JWT |
| **Dashboard** | Customer Summary, Business Summary | JWT |
| **Search** | AI-powered search | Public |
| **Waitlist** | Join, Leave, Priority, Statistics | JWT |
| **Recurring Bookings** | CRUD, Skip, Cancel Series | JWT |
| **Admin** | User/Business/Review Management, Dashboard | Admin |
| **Documents** | Upload, Download, List, Delete | JWT |
| **Settings** | User Preferences CRUD | JWT |

---

## 📱 Features

- **Provider Onboarding** — Multi-step wizard: business info, category picker, cover image, business hours, services with pricing, staff profiles
- **Business Verification Lifecycle** — New businesses start Pending, hidden from customer search until an admin approves (or rejects with reason)
- **Admin Review Queue** — Approve/reject pending businesses, view submitted verification documents
- **AI Search** — Natural language query interpretation with intelligent filtering
- **Biometric Auth** — Fingerprint / Face ID for quick login
- **Real-time Availability** — Slot generation with buffer management
- **Recurring Bookings** — Weekly, monthly, custom interval support
- **Waitlist** — Priority-based waitlist with automatic promotion
- **Reviews & Ratings** — Star ratings, replies, helpful votes, reporting
- **Multi-currency** — Configurable currency per business/user
- **Push Notifications** — Firebase Cloud Messaging integration
- **PDF Invoicing** — Generate and share booking receipts
- **Offline Support** — Connectivity monitoring with graceful degradation

---

## 📸 Screenshots

> *Screenshots will be added after the app is deployed.*

| Screen | Description |
|--------|-------------|
| Splash | Animated logo intro |
| Onboarding | Feature highlights carousel |
| Register/Login | Email + password or social sign-in |
| Home | Personalized feed with nearby businesses |
| Search | AI-powered with filters |
| Business Detail | Services, reviews, map, availability |
| Booking | Date/time picker with slot grid |
| Checkout | Review, apply promo, pay |
| Appointments | Calendar/list view of bookings |
| Profile | Edit info, biometric toggle |
| Settings | Preferences, theme, notifications |

---

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Coding Conventions

- **C#**: Follow .NET team conventions (file-scoped namespaces, primary constructors)
- **Dart**: Follow Flutter style guide with 80 char line limit
- **Commits**: Conventional commits (`feat:`, `fix:`, `docs:`, `chore:`)

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

---

## 📬 Contact

Project Link: [https://github.com/username/bookify](https://github.com/username/bookify)

---

*Built with ❤️ using .NET 10 and Flutter 3.44*

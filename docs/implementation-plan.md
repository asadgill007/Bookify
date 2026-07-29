# Bookify — Implementation Plan

## Phase 1: Project Analysis (Current)

**Goal:** Analyze design assets and produce architecture documents.

**Deliverables:**
- ✅ `docs/architecture.md` — System architecture, tech stack, cross-cutting concerns
- ✅ `docs/database-design.md` — Full database schema with 20 tables
- ✅ `docs/api-spec.md` — Complete API specification with all endpoints
- ⬜ `docs/implementation-plan.md` — This document
- ⬜ `docs/project-roadmap.md` — Phase timeline and milestones

**Files Created:**
- `docs/architecture.md` (NEW)
- `docs/database-design.md` (NEW)
- `docs/api-spec.md` (NEW)
- `docs/implementation-plan.md` (NEW)
- `docs/project-roadmap.md` (NEW)

---

## Phase 2: Backend Foundation

**Goal:** Create the ASP.NET Core solution with Clean Architecture, compilable and functional.

**Tasks:**
1. Create .NET 10 solution file: `Bookify.sln`
2. Create all 4 projects:
   - `src/Bookify.Domain/` — Class library
   - `src/Bookify.Application/` — Class library
   - `src/Bookify.Infrastructure/` — Class library
   - `src/Bookify.WebApi/` — Web API project
3. Configure solution dependencies:
   - Domain → (none)
   - Application → Domain
   - Infrastructure → Application, Domain
   - WebApi → Application, Infrastructure
4. Add NuGet packages:
   - MediatR, FluentValidation, Mapster, Serilog, EF Core, JWT, Swagger, Health Checks, Rate Limiting
5. Set up Program.cs:
   - Serilog configuration (Console + File sinks)
   - CORS policy
   - Swagger with JWT auth
   - Health checks
   - Rate limiting
   - Global exception handling middleware
   - API versioning
6. Create domain entities (no behavior yet):
   - User, Business, Provider, Service, Appointment, Review, Notification, etc.
   - Value objects: Money, Address, GeoLocation, TimeRange, Email, PhoneNumber
   - Enums: AppointmentStatus, UserRole, BusinessCategory, BookingType, PaymentStatus
7. Create Application layer contracts:
   - Repository interfaces
   - Service interfaces
   - CQRS base classes
8. Create Infrastructure layer:
   - AppDbContext with EntityConfigurations
   - EF Core migrations setup
   - Repository implementations (base + specific)
9. Create WebApi controllers:
   - BaseController with common patterns
   - AuthController scaffold
   - HealthController
10. Verify project builds successfully

**Files Created (~50):**
- `backend/Bookify.sln`
- `backend/src/Bookify.Domain/*` (15+ files)
- `backend/src/Bookify.Application/*` (20+ files)
- `backend/src/Bookify.Infrastructure/*` (15+ files)
- `backend/src/Bookify.WebApi/*` (10+ files)

---

## Phase 3: Database & Persistence

**Goal:** Full database schema with EF Core, migrations, and seed data.

**Tasks:**
1. Create all EntityTypeConfigurations with:
   - Primary keys (Guid)
   - Required/optional constraints
   - Max length constraints
   - Indexes (including composite and unique)
   - Foreign key relationships
   - Soft delete query filters
2. Complete AppDbContext:
   - DbSet properties for all entities
   - Override SaveChanges for audit columns
   - Global query filters for IsDeleted
3. Create initial migration:
   - All 20+ tables
   - Seed data for categories, admin user, currencies
4. Generate SQL scripts:
   - `database/schema.sql` — Full DDL
   - `database/seed.sql` — Seed data
   - `database/indexes.sql` — Additional indexes
5. Verify migration applies successfully
6. Add seed data for:
   - Categories & subcategories
   - Admin user
   - Sample businesses, providers, services (for development)
7. Add repository implementations for all entities
8. Implement unit of work pattern

**Files Created/Modified:**
- `backend/src/Bookify.Infrastructure/Persistence/*` (10+ files)
- `backend/src/Bookify.WebApi/Program.cs` (modified)
- `database/schema.sql` (NEW)
- `database/seed.sql` (NEW)
- `database/indexes.sql` (NEW)

---

## Phase 4: Core Backend Features

**Goal:** Implement all business logic — auth, users, businesses, providers, appointments, reviews, notifications, dashboard, settings, AI.

### Sub-Phase 4.1: Authentication & Authorization
- JWT token generation + refresh token rotation
- Password hashing (PBKDF2)
- Register, Login, Refresh, Logout, Forgot/Reset Password
- Policy-based authorization (Customer, Provider, BusinessOwner, Admin)
- Resource ownership checks

### Sub-Phase 4.2: User Management
- Get/Update profile
- Change password
- Delete account (GDPR)
- Biometric toggle

### Sub-Phase 4.3: Business Management
- CRUD for businesses (Owner only)
- Business verification (Admin only)
- Business search with pagination, sorting, filtering, geo-location
- Business images/gallery management
- Business categories assignment

### Sub-Phase 4.4: Provider Management
- Provider CRUD
- Provider availability management (weekly recurring + date overrides)
- Slot generation algorithm (given availability, service duration, existing bookings)

### Sub-Phase 4.5: Appointment Booking
- Create appointment with conflict detection
- Status flow: Pending → Confirmed → InProgress → Completed
- Cancel, Reschedule, Confirm, Complete endpoints
- Booking reference generation
- Appointment audit log
- Concurrent booking protection (optimistic concurrency)

### Sub-Phase 4.6: Reviews
- Submit review (only after completed appointment, one per appointment)
- Update/Delete own review
- Review moderation (Admin)
- Auto-update business average rating (denormalized)

### Sub-Phase 4.7: Payments
- Payment initialization
- Payment confirmation
- Refund processing
- Payment history
- Multiple payment methods support

### Sub-Phase 4.8: Notifications
- In-app notification creation
- Mark read/unread
- Push notification infrastructure
- Appointment reminders (background job)

### Sub-Phase 4.9: Dashboard
- Customer dashboard (upcoming, history, stats)
- Business owner dashboard (revenue, booking counts, analytics)

### Sub-Phase 4.10: Settings
- User preferences (language, currency, theme, notifications)
- Biometric settings

### Sub-Phase 4.11: AI Foundation
- AI search service interface + mock implementation
- Natural language query parsing
- Search result ranking

**Files Created/Modified (~100+ files across all sub-phases)**

---

## Phase 5: Flutter Foundation

**Goal:** Create the Flutter project with all infrastructure, no business screens yet.

**Tasks:**
1. Create Flutter project: `flutter create bookify_mobile`
2. Set up folder structure (as defined in architecture.md)
3. Add dependencies to `pubspec.yaml`:
   - flutter_riverpod, riverpod_annotation
   - dio, retrofit
   - go_router
   - hive, hive_flutter
   - flutter_secure_storage
   - freezed_annotation, json_annotation
   - build_runner, freezed, json_serializable
   - flutter_localizations (intl)
   - cached_network_image, shimmer
   - flutter_local_auth (biometric)
   - flutter_animate (animations)
4. Create design system:
   - Light theme (bookify_premium_narrative colors)
   - Dark theme (nocturne_luxury colors)
   - AMOLED dark mode variant
   - Typography (Plus Jakarta Sans + Inter for light; Manrope + Geist for dark)
   - Glassmorphic widget theme
   - Custom ThemeExtension for glass styles
5. Create reusable widgets:
   - `GlassCard` — Glassmorphic container with blur
   - `GlassAppBar` — Translucent top bar
   - `SearchBar` — Pill-shaped with voice button
   - `CategoryIcon` — Circular category icon
   - `BusinessCard` — Featured business card
   - `BottomNavBar` — Glass floating bottom nav
   - `PrimaryButton`, `SecondaryButton`, `TertiaryButton`
   - `LoadingShimmer` — Skeleton loading
   - `ErrorView`, `EmptyState`
   - `RatingBadge`, `PriceTag`, `StatusChip`
   - `DateSelector` — Horizontal date scroller
   - `TimeSlotGrid` — Time slot selection grid
6. Set up routing:
   - GoRouter with nested routes
   - Route guards (auth required, role-based)
   - Deep linking support
7. Set up networking:
   - Dio client with interceptors (auth, logging, retry)
   - Token refresh interceptor
   - Base API configuration
8. Set up local storage:
   - Hive boxes for cache
   - Secure storage for tokens
9. Set up state management:
   - Riverpod providers for auth state, theme mode, locale
10. Set up localization:
    - ARB files for English, Arabic, French, Spanish
    - Dynamic locale switching
11. Set up analytics & crash reporting foundation

**Files Created (~80+)**

---

## Phase 6: Flutter UI Implementation

**Goal:** Implement every screen from the design assets, pixel-perfect.

### Screen 1: Onboarding (2 variants)
- Light mode onboarding (bookify_premium_narrative)
- Dark mode onboarding (nocturne_luxury)
- Animated hero with floating elements
- Glassmorphic card with value props
- "Get Started" + "Sign In" CTAs
- Floating indicator cards (Luxury Spa, Modern Clinics)

### Screen 2: Personalization
- Language selection chips
- Currency selection chips
- Interests multi-select grid
- Continue button
- Background decorative elements

### Screen 3: Home / Discovery (2 variants)
- Top app bar with location
- Hero search with voice button
- Horizontal categories scroll
- "Top Rated Nearby" grid
- Promotion bento section (Bookify Plus, Referral)
- Bottom navigation bar (Home, Search, Bookings, Chat, Profile)

### Screen 4: AI Search Results (2 variants)
- Search bar with active filters
- Filter chips (distance, price, rating)
- Results grid with business cards
- Each card: image, rating badge, name, distance, price, tags, book button
- "Next Available" badge animation

### Screen 5: Business/Provider Profile (2 variants)
- Hero banner image with gradient overlay
- Business info header
- Quick actions (Chat, Call, Route)
- Asymmetric gallery grid
- Services list grouped by category
- Sidebar with location map + host card (desktop)
- Sticky bottom booking bar

### Screen 6: Time Slot Selection (2 variants)
- Progress stepper (Service → Time → Payment)
- Provider summary card
- Horizontal date scroller
- Time slot grid (Morning, Afternoon, Evening)
- Disabled/unavailable slots
- Selected slot summary footer
- Confirm Time button

### Screen 7: Checkout & Payments (2 variants)
- Booking summary card
- Price breakdown
- Payment method selection (Apple Pay, Google Pay, Credit Card, PayPal, Wallet)
- Billing address section
- Promo code input
- Total calculation
- Confirm Payment button

### Screen 8: Confirmation / Digital Ticket (2 variants)
- Success animation
- QR code / booking reference
- Appointment details
- Add to calendar button
- Share button
- Directions button
- Reschedule/Cancel options

### Screen 9: Customer Dashboard
- Upcoming appointments list
- Past appointments history
- Booking stats summary
- Notification badges

### Screen 10: Settings
- Profile section
- Preferences (language, currency, theme)
- Notification toggles
- Biometric toggle
- Privacy & security
- About & help

### Screen 11: Chat
- Conversation list
- Message thread with provider
- AI assistant integration

### Screen 12: Notifications
- Notification list with grouping
- Read/unread states
- Swipe to delete

**Files Created (~100+ Dart files)**

---

## Phase 7: API Integration

**Goal:** Connect Flutter with the backend API.

**Tasks:**
1. Generate API client from OpenAPI spec (or manual Dio service classes)
2. Implement all data models with JSON serialization
3. Implement all repository interfaces
4. Implement auth flow:
   - Login with token storage
   - Auto-refresh on 401
   - Biometric authentication
5. Implement booking flow end-to-end:
   - Search → Profile → Select Time → Checkout → Confirmation
6. Implement offline caching:
   - Cache business search results in Hive
   - Cache categories
   - Queue failed appointment creation
   - Sync when online
7. Implement error handling:
   - Retry logic (3 attempts with exponential backoff)
   - Network connectivity monitoring
   - User-friendly error messages
8. Implement loading states:
   - Skeleton shimmer loading
   - Pull-to-refresh
   - Infinite scroll pagination

**Files Modified (~60+ existing + new service files)**

---

## Phase 8: Final Polish

**Goal:** Production readiness.

**Tasks:**
1. Performance optimization:
   - API response caching
   - Image lazy loading + caching
   - List view optimization (ListView.builder, pagination)
   - Reduce widget rebuilds
2. Security review:
   - Penetration test checklist
   - JWT token security
   - API rate limiting verification
   - Input validation audit
3. Code cleanup:
   - Remove all warnings
   - Consistent code style (EditorConfig)
   - Remove debug code
   - Finalize TODO comments
4. Accessibility:
   - Screen reader support (Semantics)
   - Sufficient color contrast
   - Touch target sizes (48x48 minimum)
   - Font scaling support
5. Testing:
   - Unit tests for Application layer (xUnit)
   - Unit tests for Domain entities
   - Integration tests for API endpoints
   - Widget tests for Flutter screens
   - Golden tests for visual regression
6. Documentation:
   - README.md for backend and mobile
   - API documentation comments
   - Setup guide
   - Deployment guide

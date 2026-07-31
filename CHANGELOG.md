# Changelog

All notable changes to the Bookify project are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### Backend — Provider & Business Onboarding
- `POST /auth/register` now accepts an `accountType` field (`customer`, `provider`, `businessOwner`). Public registration can never assign `admin`.
- `POST /api/v1/businesses` accepts real category IDs (`categoryIds`), a cover image URL, address line 2, state, and cancellation policy, with slug-conflict detection.
- `PUT /api/v1/businesses/{id}` — update an existing business (info, address, geo, categories).
- `PUT /api/v1/businesses/{id}/hours` — set weekly business opening hours (`BusinessHours` entity).
- `POST /api/v1/businesses/{id}/services` — add services with name, description, duration, price, currency.
- `POST /api/v1/businesses/{id}/providers` — add staff/providers (with default Mon–Fri 09:00–18:00 + Sat 10:00–15:00 availability so new businesses are immediately bookable).
- `GET /api/v1/businesses/mine` — list businesses owned by the current user.
- `POST /api/v1/businesses/{id}/resubmit` — allow a rejected business owner to resubmit for review.
- `POST /api/v1/admin/businesses/{id}/verify` / `reject` — full verification lifecycle (`Pending → Approved / Rejected` with rejection reason), replacing the plain boolean toggle. Admin queue can filter by `status=Pending|Approved|Rejected`.
- `GET /api/v1/documents/business/{businessId}` — admin can view submitted verification documents during review.
- `GET /api/v1/businesses` public search now shows **verified businesses only** by default; pending/rejected businesses stay hidden until approved. AI search honors the same rule.
- `GET /api/v1/businesses/{slug}` returns `VerificationStatus`, `RejectionReason`, and `OpeningHours`; pending/rejected businesses are only visible to their owner or admins.

#### Frontend (Flutter)
- Registration screen: account-type selection step (Customer vs. "List your business").
- Provider onboarding wizard: business info → categories → cover image → business hours → services with pricing → staff profiles.
- "My Business" dashboard with a **pending verification** state, rejection reason, and resubmit action.
- Admin review screen: pending-business queue with documents viewer and Approve / Reject (with required reason).
- Home screen wired to real categories and verified-business listings.
- Business detail screen wired to the real API by slug (services, providers, hours).
- Booking flow wired to real services/providers/slot APIs; checkout creates a real appointment; confirmation shows the real booking reference.
- Router, auth provider, and profile screen updated for the new roles/flows.

### Fixed
- **Critical:** `BaseRepository<T>.GetByIdAsync` returned an *untracked* entity (`AsNoTracking`), so all update flows (admin verify/reject, business update, hours, toggle-active, resubmit) silently persisted nothing. Now tracked — mutations persist. Verified end-to-end.
- **Search:** business search was case-sensitive under the InMemory provider but case-insensitive under SQL Server. Normalized with case-insensitive matching so `search=e2e` and `search=E2E` behave identically across environments.

### Changed
- `Business.Verify` records `ReviewedAt`/`ReviewedBy` and publishes the `BusinessVerifiedEvent` domain event (now actually delivered, thanks to the tracking fix).
- Rejection now requires a non-empty reason (server-validated).

### Security & Operations
- `.gitignore` covers all `api_*.txt` runtime logs, `*_stdout.txt` / `*_stderr.txt`.
- Removed committed runtime artifacts (`write_booking.js`, `backend/api_seed_error.txt`).

## [0.1.0] - 2026-07-31

### Added
- Initial release: ASP.NET Core 10 backend (Clean Architecture + CQRS/MediatR), Flutter frontend, JWT auth with refresh rotation, seeded demo data, AI search, slot generation, reviews, payments, notifications, admin dashboard, and Swagger docs.

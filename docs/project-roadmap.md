# Bookify — Project Roadmap

## Overview

| Phase | Name | Duration (Est.) | Deliverables | Dependencies |
|---|---|---|---|---|
| 1 | **Project Analysis** | 1 day | Architecture docs, DB design, API spec | None |
| 2 | **Backend Foundation** | 3-4 days | Compilable .NET 10 solution | Phase 1 |
| 3 | **Database & Persistence** | 2-3 days | EF Core migrations, SQL scripts | Phase 2 |
| 4 | **Core Backend Features** | 7-10 days | All business logic complete | Phase 3 |
| 5 | **Flutter Foundation** | 3-4 days | Compilable Flutter project | None (parallel to Phase 2-4) |
| 6 | **Flutter UI Implementation** | 7-10 days | All screens implemented | Phase 5 |
| 7 | **API Integration** | 5-7 days | Full-stack integration | Phase 4 + Phase 6 |
| 8 | **Final Polish** | 3-5 days | Production-ready app | Phase 7 |

**Total Estimated Timeline:** 31-44 days

---

## Phase 1: Project Analysis ✅ (Current)

| Task | Status |
|---|---|
| Read design assets (PRD, DESIGN.md, screens) | ✅ Complete |
| Produce `architecture.md` | ✅ Complete |
| Produce `database-design.md` | ✅ Complete |
| Produce `api-spec.md` | ✅ Complete |
| Produce `implementation-plan.md` | ✅ Complete |
| Produce `project-roadmap.md` | ✅ Complete |
| **GATE: Approve Phase 1 deliverables** | ⬜ **Pending** |

---

## Phase 2: Backend Foundation

| Milestone | Tasks |
|---|---|
| **M2.1: Solution Structure** | Create .sln, 4 projects, project references |
| **M2.2: NuGet Configuration** | Add all packages, verify restore |
| **M2.3: Domain Layer** | Entities, ValueObjects, Enums, DomainEvents |
| **M2.4: Application Layer** | Interfaces, CQRS base, DTOs, Validators |
| **M2.5: Infrastructure Layer** | DbContext, Repositories, Auth services |
| **M2.6: WebApi Layer** | Program.cs, Middleware, BaseController, Health |
| **M2.7: Build Gate** | `dotnet build` passes with zero warnings |

---

## Phase 3: Database & Persistence

| Milestone | Tasks |
|---|---|
| **M3.1: Entity Configurations** | Fluent API for all 20+ entities |
| **M3.2: AppDbContext** | DbSets, SaveChanges overrides, query filters |
| **M3.3: Initial Migration** | `dotnet ef migrations add Initial` |
| **M3.4: Seed Data** | Categories, Admin, sample data |
| **M3.5: SQL Scripts** | schema.sql, seed.sql, indexes.sql |
| **M3.6: Repositories** | Full repository implementations |
| **M3.7: Build Gate** | `dotnet build` passes |

---

## Phase 4: Core Backend Features

| Milestone | Tasks | Est. Days |
|---|---|---|
| **M4.1: Auth** | Register, Login, Refresh, JWT, Policies | 1.5 |
| **M4.2: Users** | Profile CRUD, password, GDPR delete | 0.5 |
| **M4.3: Businesses** | CRUD, search, geo, gallery, categories | 1.5 |
| **M4.4: Providers** | CRUD, availability, slot generation | 1.0 |
| **M4.5: Appointments** | CRUD, status flow, conflict detection | 1.5 |
| **M4.6: Reviews** | Submit, update, auto-rating calculation | 0.5 |
| **M4.7: Payments** | Initialize, confirm, refund, history | 1.0 |
| **M4.8: Notifications** | CRUD, background reminders | 0.5 |
| **M4.9: Dashboard** | Customer + Owner dashboards | 1.0 |
| **M4.10: Settings** | Preferences, biometric toggle | 0.5 |
| **M4.11: AI** | AI search interface, NLP mock | 0.5 |
| **M4.12: Build + Test Gate** | Build passes, unit tests green | 0.5 |

---

## Phase 5: Flutter Foundation

| Milestone | Tasks |
|---|---|
| **M5.1: Project Creation** | `flutter create`, folder structure |
| **M5.2: Dependencies** | pubspec.yaml with all packages |
| **M5.3: Design System** | Light/Dark/Amoled themes, typography, glass styles |
| **M5.4: Reusable Widgets** | GlassCard, SearchBar, CategoryIcon, BottomNav, Buttons |
| **M5.5: Routing** | GoRouter with auth guards |
| **M5.6: Networking** | Dio client, interceptors, retry |
| **M5.7: State Management** | Riverpod providers for global state |
| **M5.8: Storage** | Hive boxes, secure storage |
| **M5.9: Localization** | ARB files, locale switching |
| **M5.10: Build Gate** | `flutter build` passes |

---

## Phase 6: Flutter UI Implementation

| Milestone | Screens | Est. Days |
|---|---|---|
| **M6.1: Onboarding** | Light + Dark onboarding | 0.5 |
| **M6.2: Personalization** | Preferences screen | 0.5 |
| **M6.3: Home/Discovery** | Light + Dark home | 1.0 |
| **M6.4: Search Results** | Light + Dark AI search | 1.0 |
| **M6.5: Business Profile** | Light + Dark business detail | 1.5 |
| **M6.6: Time Slot Selection** | Light + Dark booking flow | 1.0 |
| **M6.7: Checkout** | Light + Dark payment sheet | 1.0 |
| **M6.8: Confirmation** | Light + Dark digital ticket | 0.5 |
| **M6.9: Dashboard** | Customer dashboard | 0.5 |
| **M6.10: Settings** | Settings & preferences | 0.5 |
| **M6.11: Chat** | Chat screens | 0.5 |
| **M6.12: Notifications** | Notification screen | 0.5 |
| **M6.13: Build Gate** | All screens render without errors | 0.5 |

---

## Phase 7: API Integration

| Milestone | Tasks | Est. Days |
|---|---|---|
| **M7.1: API Client** | Generated/service classes for all endpoints | 1.0 |
| **M7.2: Auth Flow** | Login, token storage, auto-refresh, biometric | 0.5 |
| **M7.3: Booking Flow E2E** | Search → Book → Pay → Confirm | 1.5 |
| **M7.4: Offline Caching** | Hive cache, sync queue | 1.0 |
| **M7.5: Error Handling** | Retry logic, connectivity, user-facing errors | 0.5 |
| **M7.6: Integration Test** | Full flow test | 0.5 |

---

## Phase 8: Final Polish

| Milestone | Tasks | Est. Days |
|---|---|---|
| **M8.1: Performance** | Caching, lazy loading, widget optimization | 1.0 |
| **M8.2: Security** | Pen-test checklist, auth audit, input validation | 0.5 |
| **M8.3: Code Cleanup** | Remove warnings, format, finalize | 0.5 |
| **M8.4: Accessibility** | Semantics, contrast, touch targets | 0.5 |
| **M8.5: Tests** | Unit, integration, widget, golden tests | 2.0 |
| **M8.6: Documentation** | README, setup guide, deployment guide | 0.5 |
| **M8.7: Final Build Gate** | All builds pass, tests green | 0.5 |

---

## Dependency Graph

```
Phase 1 (Analysis)
    ├──> Phase 2 (Backend Foundation)
    │       └──> Phase 3 (Database)
    │               └──> Phase 4 (Backend Features)
    │                       └──> Phase 7 (API Integration) ──┐
    │                                                         ├──> Phase 8 (Polish)
    └──> Phase 5 (Flutter Foundation)                        │
            └──> Phase 6 (Flutter UI) ────────────────────────┘
```

**Optimization:** Phase 2-4 (backend) and Phase 5-6 (Flutter) can run in parallel by different developers.

---

## Key Decision Points

| Checkpoint | Decision |
|---|---|
| End of Phase 1 | Approve architecture and design decisions |
| End of Phase 2 | Approve technology choices and project structure |
| End of Phase 4 | Approve API for Flutter integration |
| End of Phase 6 | Approve UI before integration |
| End of Phase 7 | Approve end-to-end functionality |
| End of Phase 8 | Final sign-off for production |

---

## Risk Register

| Risk | Impact | Probability | Mitigation |
|---|---|---|---|
| .NET 10 preview instability | High | Low | Fall back to .NET 9 if blocking issues arise |
| Flutter package breaking changes | Medium | Medium | Pin exact versions, use dependency locking |
| SQL Server licensing cost | Low | High | Use SQL Server Developer Edition for dev; consider PostgreSQL if needed |
| Payment gateway integration complexity | Medium | Medium | Abstract behind IPaymentService; implement mock first |
| AI search quality | Medium | Medium | Start with simple keyword search; iterative AI improvements |
| Third-party API rate limits | Medium | Low | Implement caching and queuing |

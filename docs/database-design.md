# Bookify — Database Design

## 1. Design Principles

- **Normalization to 3NF** with selective denormalization for read performance
- **UUID primary keys** (Guid) for distributed generation and security (no sequential IDs)
- **Soft deletes** (IsDeleted flag + DeletedAt + DeletedBy) on all user-facing entities
- **Audit columns** (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy) on every table
- **RowVersion concurrency tokens** on key entities (User, Business, Appointment) for optimistic concurrency
- **Check constraints** enforcing business rules (Rating 1-5, Price >= 0, SlotDuration 15-480 min, TimeRange End > Start)
- **Composite indexes** on all foreign keys and frequently-queried columns
- **Descending indexes** on sort columns (CreatedAt DESC, AverageRating DESC)
- **Full-text indexes** on searchable text columns (Business.Name, Service.Name, Business.Description)
- **Spatial indexes** on GeoLocation columns for location-based queries
- **Row-level security** considerations for multi-tenant data isolation

---

## 2. Entity Relationship Diagram (Textual)

```
Users ──1:N──> Businesses           (Businesses owned by Users)
Users ──1:N──> Reviews              (Reviews written by Users)
Users ──1:N──> Notifications        (Notifications sent to Users)
Users ──1:N──> RefreshTokens        (Refresh tokens owned by Users)
Users ──1:N──> UserPreferences      (Language, Currency, Interests)

Businesses ──1:N──> Providers       (Providers working at a Business)
Businesses ──1:N──> Services        (Services offered by a Business)
Businesses ──1:N──> Reviews         (Reviews received by a Business)
Businesses ──1:N──> BusinessImages  (Gallery images for a Business)
Businesses ──1:N──> BusinessCategories (Category assignments)

Providers ──1:N──> ProviderAvailability (Weekly recurring schedule)
Providers ──1:N──> Appointments     (Appointments assigned to a Provider)
Providers ──1:N──> ProviderServices (Services a Provider can perform)

Services ──N:N──> Providers         (via ProviderServices)
Services ──1:N──> Appointments      (Appointments for a Service)

Appointments ──1:1──> Payments      (Payment for an appointment)
Appointments ──1:1──> Review         (Review left for an appointment)
Appointments ──1:N──> AppointmentLogs (Audit trail of status changes)

Payments ──1:N──> PaymentTransactions (Raw transaction records)

Categories ──1:N──> BusinessCategories (Linking categories to businesses)
Categories ──1:N──> SubCategories    (Hierarchical category structure)
```

---

## 3. Table Definitions

### 3.1 Users

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK, DEFAULT NEWSEQUENTIALID() | |
| FirstName | NVARCHAR(100) | NOT NULL | |
| LastName | NVARCHAR(100) | NOT NULL | |
| Email | NVARCHAR(256) | NOT NULL, UNIQUE | |
| PhoneNumber | NVARCHAR(20) | NULL | |
| PasswordHash | NVARCHAR(500) | NOT NULL | PBKDF2 hash |
| Role | INT | NOT NULL, DEFAULT 0 | 0=Customer, 1=Provider, 2=BusinessOwner, 3=Admin |
| AvatarUrl | NVARCHAR(1000) | NULL | |
| IsBiometricEnabled | BIT | NOT NULL, DEFAULT 0 | |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 | Soft delete |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT SYSUTCDATETIME() | |
| UpdatedAt | DATETIME2 | NOT NULL, DEFAULT SYSUTCDATETIME() | Auto-updated |
| LastLoginAt | DATETIME2 | NULL | |
| PreferredLanguage | NVARCHAR(10) | NOT NULL, DEFAULT 'en' | |
| PreferredCurrency | NVARCHAR(3) | NOT NULL, DEFAULT 'USD' | ISO 4217 |

**Indexes:**
- `IX_Users_Email` UNIQUE on (Email) WHERE IsDeleted = 0
- `IX_Users_Role` on (Role)

---

### 3.2 RefreshTokens

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| UserId | UNIQUEIDENTIFIER | FK → Users.Id, NOT NULL | |
| Token | NVARCHAR(500) | NOT NULL | |
| JwtId | NVARCHAR(200) | NOT NULL | Maps to JWT jti claim |
| IsUsed | BIT | NOT NULL, DEFAULT 0 | |
| IsRevoked | BIT | NOT NULL, DEFAULT 0 | |
| CreatedAt | DATETIME2 | NOT NULL | |
| ExpiresAt | DATETIME2 | NOT NULL | |

**Indexes:**
- `IX_RefreshTokens_Token` on Token (for lookup)
- `IX_RefreshTokens_UserId` on UserId

---

### 3.3 Businesses

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| OwnerId | UNIQUEIDENTIFIER | FK → Users.Id, NOT NULL | Business owner |
| Name | NVARCHAR(200) | NOT NULL | |
| Slug | NVARCHAR(250) | NOT NULL, UNIQUE | URL-friendly name |
| Description | NVARCHAR(2000) | NULL | |
| Email | NVARCHAR(256) | NULL | Business contact email |
| PhoneNumber | NVARCHAR(20) | NULL | |
| Address_Line1 | NVARCHAR(200) | NOT NULL | Value Object stored as column |
| Address_Line2 | NVARCHAR(200) | NULL | |
| Address_City | NVARCHAR(100) | NOT NULL | |
| Address_State | NVARCHAR(100) | NULL | |
| Address_PostalCode | NVARCHAR(20) | NOT NULL | |
| Address_Country | NVARCHAR(100) | NOT NULL | |
| GeoLocation_Latitude | FLOAT | NULL | Spatial query support |
| GeoLocation_Longitude | FLOAT | NULL | |
| Website | NVARCHAR(500) | NULL | |
| IsVerified | BIT | NOT NULL, DEFAULT 0 | |
| IsActive | BIT | NOT NULL, DEFAULT 1 | |
| BookingType | INT | NOT NULL, DEFAULT 0 | 0=Instant, 1=ApprovalRequired |
| CancellationPolicy | NVARCHAR(2000) | NULL | |
| TimeZone | NVARCHAR(100) | NOT NULL, DEFAULT 'UTC' | IANA timezone |
| Currency | NVARCHAR(3) | NOT NULL, DEFAULT 'USD' | |
| AverageRating | DECIMAL(2,1) | NOT NULL, DEFAULT 0.0 | Denormalized |
| TotalReviews | INT | NOT NULL, DEFAULT 0 | Denormalized |
| CoverImageUrl | NVARCHAR(1000) | NULL | Hero banner image |
| LogoUrl | NVARCHAR(1000) | NULL | |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 | |
| CreatedAt | DATETIME2 | NOT NULL | |
| UpdatedAt | DATETIME2 | NOT NULL | |

**Indexes:**
- `IX_Businesses_Slug` UNIQUE on (Slug) WHERE IsDeleted = 0
- `IX_Businesses_OwnerId` on OwnerId
- `IX_Businesses_GeoLocation` SPATIAL on GeoLocation
- `IX_Businesses_City_Country` on (Address_City, Address_Country)
- `IX_Businesses_AverageRating` on AverageRating DESC
- Full-text index on (Name, Description)

---

### 3.4 Categories

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| Name | NVARCHAR(100) | NOT NULL | e.g., "Doctors", "Salons", "Spas" |
| Slug | NVARCHAR(150) | NOT NULL, UNIQUE | |
| IconName | NVARCHAR(100) | NULL | Material icon name |
| DisplayOrder | INT | NOT NULL, DEFAULT 0 | |
| IsActive | BIT | NOT NULL, DEFAULT 1 | |

---

### 3.5 BusinessCategories

| Column | Type | Constraints | Notes |
|---|---|---|---|
| BusinessId | UNIQUEIDENTIFIER | FK → Businesses.Id, NOT NULL | Composite PK |
| CategoryId | UNIQUEIDENTIFIER | FK → Categories.Id, NOT NULL | Composite PK |

**Indexes:**
- `IX_BusinessCategories_CategoryId` on CategoryId

---

### 3.6 BusinessImages

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| BusinessId | UNIQUEIDENTIFIER | FK → Businesses.Id, NOT NULL | |
| Url | NVARCHAR(1000) | NOT NULL | |
| AltText | NVARCHAR(500) | NULL | |
| DisplayOrder | INT | NOT NULL, DEFAULT 0 | |
| IsCover | BIT | NOT NULL, DEFAULT 0 | |
| CreatedAt | DATETIME2 | NOT NULL | |

**Indexes:**
- `IX_BusinessImages_BusinessId_DisplayOrder` on (BusinessId, DisplayOrder)

---

### 3.7 Providers

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| UserId | UNIQUEIDENTIFIER | FK → Users.Id, NOT NULL | Link to user account |
| BusinessId | UNIQUEIDENTIFIER | FK → Businesses.Id, NOT NULL | |
| Title | NVARCHAR(200) | NULL | e.g., "Senior Wellbeing Specialist" |
| Bio | NVARCHAR(2000) | NULL | |
| IsActive | BIT | NOT NULL, DEFAULT 1 | |
| DisplayOrder | INT | NOT NULL, DEFAULT 0 | |

**Indexes:**
- `IX_Providers_BusinessId` on BusinessId
- `IX_Providers_UserId` UNIQUE on UserId

---

### 3.8 ProviderAvailability (Weekly Recurring)

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| ProviderId | UNIQUEIDENTIFIER | FK → Providers.Id, NOT NULL | |
| DayOfWeek | INT | NOT NULL | 0=Sunday..6=Saturday |
| StartTime | TIME | NOT NULL | |
| EndTime | TIME | NOT NULL | |
| IsAvailable | BIT | NOT NULL, DEFAULT 1 | |
| SlotDurationMinutes | INT | NOT NULL, DEFAULT 60 | Duration of each appointment slot |

**Indexes:**
- `IX_ProviderAvailability_ProviderId_DayOfWeek` on (ProviderId, DayOfWeek)

---

### 3.9 ProviderAvailabilityOverrides (Date-Specific)

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| ProviderId | UNIQUEIDENTIFIER | FK → Providers.Id, NOT NULL | |
| Date | DATE | NOT NULL | The specific date |
| StartTime | TIME | NULL | Null means entire day |
| EndTime | TIME | NULL | |
| IsAvailable | BIT | NOT NULL | false = day off, true = extra hours |
| Reason | NVARCHAR(500) | NULL | "Holiday", "Training", etc. |

**Indexes:**
- `IX_AvailabilityOverrides_ProviderId_Date` on (ProviderId, Date)

---

### 3.10 Services

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| BusinessId | UNIQUEIDENTIFIER | FK → Businesses.Id, NOT NULL | |
| Name | NVARCHAR(200) | NOT NULL | |
| Description | NVARCHAR(2000) | NULL | |
| DurationMinutes | INT | NOT NULL | |
| Price_Amount | DECIMAL(18,2) | NOT NULL | Value Object |
| Price_Currency | NVARCHAR(3) | NOT NULL, DEFAULT 'USD' | |
| Category | NVARCHAR(100) | NULL | "Medical Consultations", "Therapy" |
| IsActive | BIT | NOT NULL, DEFAULT 1 | |
| DisplayOrder | INT | NOT NULL, DEFAULT 0 | |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 | |
| CreatedAt | DATETIME2 | NOT NULL | |
| UpdatedAt | DATETIME2 | NOT NULL | |

**Indexes:**
- `IX_Services_BusinessId` on BusinessId
- `IX_Services_Name` on Name (for search)
- Full-text index on Name, Description

---

### 3.11 ProviderServices

| Column | Type | Constraints | Notes |
|---|---|---|---|
| ProviderId | UNIQUEIDENTIFIER | FK → Providers.Id | Composite PK |
| ServiceId | UNIQUEIDENTIFIER | FK → Services.Id | Composite PK |

---

### 3.12 Appointments

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| BookingReference | NVARCHAR(20) | NOT NULL, UNIQUE | Human-readable ref |
| CustomerId | UNIQUEIDENTIFIER | FK → Users.Id, NOT NULL | |
| ProviderId | UNIQUEIDENTIFIER | FK → Providers.Id, NOT NULL | |
| ServiceId | UNIQUEIDENTIFIER | FK → Services.Id, NOT NULL | |
| BusinessId | UNIQUEIDENTIFIER | FK → Businesses.Id, NOT NULL | Denormalized for query perf |
| StartTime | DATETIME2 | NOT NULL | |
| EndTime | DATETIME2 | NOT NULL | |
| Status | INT | NOT NULL, DEFAULT 0 | 0=Pending, 1=Confirmed, 2=InProgress, 3=Completed, 4=Cancelled, 5=NoShow, 6=Rescheduled |
| CustomerNotes | NVARCHAR(1000) | NULL | |
| IsCustomerNotified | BIT | NOT NULL, DEFAULT 0 | |
| TotalAmount | DECIMAL(18,2) | NOT NULL | |
| Currency | NVARCHAR(3) | NOT NULL, DEFAULT 'USD' | |
| CancellationReason | NVARCHAR(500) | NULL | |
| RescheduledFromId | UNIQUEIDENTIFIER | FK → Appointments.Id, NULL | For rescheduling chain |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 | |
| CreatedAt | DATETIME2 | NOT NULL | |
| UpdatedAt | DATETIME2 | NOT NULL | |

**Indexes:**
- `IX_Appointments_BookingReference` UNIQUE on BookingReference
- `IX_Appointments_CustomerId` on CustomerId
- `IX_Appointments_ProviderId` on ProviderId
- `IX_Appointments_BusinessId` on BusinessId
- `IX_Appointments_StartTime_Status` on (StartTime, Status)

---

### 3.13 AppointmentLogs

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| AppointmentId | UNIQUEIDENTIFIER | FK → Appointments.Id, NOT NULL | |
| FromStatus | INT | NULL | |
| ToStatus | INT | NOT NULL | |
| ChangedByUserId | UNIQUEIDENTIFIER | FK → Users.Id, NULL | |
| Reason | NVARCHAR(500) | NULL | |
| CreatedAt | DATETIME2 | NOT NULL | |

**Indexes:**
- `IX_AppointmentLogs_AppointmentId` on AppointmentId

---

### 3.14 Payments

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| AppointmentId | UNIQUEIDENTIFIER | FK → Appointments.Id, NOT NULL, UNIQUE | |
| CustomerId | UNIQUEIDENTIFIER | FK → Users.Id, NOT NULL | |
| Amount | DECIMAL(18,2) | NOT NULL | |
| Currency | NVARCHAR(3) | NOT NULL | |
| PaymentMethod | INT | NOT NULL | 0=CreditCard, 1=DebitCard, 2=PayPal, 3=ApplePay, 4=GooglePay, 5=Wallet, 6=BankTransfer |
| Status | INT | NOT NULL | 0=Pending, 1=Authorized, 2=Captured, 3=Refunded, 4=Failed, 5=PartiallyRefunded |
| TransactionId | NVARCHAR(200) | NULL | External PSP transaction ID |
| IsDeposit | BIT | NOT NULL, DEFAULT 0 | Deposit vs full payment |
| RefundAmount | DECIMAL(18,2) | NULL | |
| RefundReason | NVARCHAR(500) | NULL | |
| CreatedAt | DATETIME2 | NOT NULL | |
| UpdatedAt | DATETIME2 | NOT NULL | |

**Indexes:**
- `IX_Payments_AppointmentId` UNIQUE on AppointmentId
- `IX_Payments_TransactionId` on TransactionId

---

### 3.15 PaymentTransactions

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| PaymentId | UNIQUEIDENTIFIER | FK → Payments.Id, NOT NULL | |
| Action | NVARCHAR(50) | NOT NULL | "Authorization", "Capture", "Refund" |
| Amount | DECIMAL(18,2) | NOT NULL | |
| ProviderResponse | NVARCHAR(MAX) | NULL | Raw PSP response |
| Status | NVARCHAR(20) | NOT NULL | "Success", "Failed" |
| CreatedAt | DATETIME2 | NOT NULL | |

---

### 3.16 Reviews

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| AppointmentId | UNIQUEIDENTIFIER | FK → Appointments.Id, NOT NULL, UNIQUE | One review per appointment |
| BusinessId | UNIQUEIDENTIFIER | FK → Businesses.Id, NOT NULL | Denormalized |
| CustomerId | UNIQUEIDENTIFIER | FK → Users.Id, NOT NULL | |
| Rating | INT | NOT NULL, CHECK (1-5) | |
| Comment | NVARCHAR(2000) | NULL | |
| IsVerifiedPurchase | BIT | NOT NULL, DEFAULT 1 | |
| IsPublished | BIT | NOT NULL, DEFAULT 1 | |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 | |
| CreatedAt | DATETIME2 | NOT NULL | |
| UpdatedAt | DATETIME2 | NOT NULL | |

**Indexes:**
- `IX_Reviews_AppointmentId` UNIQUE on AppointmentId
- `IX_Reviews_BusinessId` on BusinessId
- `IX_Reviews_CustomerId` on CustomerId

---

### 3.17 Notifications

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| UserId | UNIQUEIDENTIFIER | FK → Users.Id, NOT NULL | |
| Type | INT | NOT NULL | 0=AppointmentReminder, 1=BookingConfirmed, 2=BookingCancelled, 3=Promotion, 4=ChatMessage, 5=System |
| Title | NVARCHAR(200) | NOT NULL | |
| Body | NVARCHAR(2000) | NOT NULL | |
| Data | NVARCHAR(MAX) | NULL | JSON payload |
| IsRead | BIT | NOT NULL, DEFAULT 0 | |
| CreatedAt | DATETIME2 | NOT NULL | |

**Indexes:**
- `IX_Notifications_UserId_IsRead` on (UserId, IsRead)
- `IX_Notifications_UserId_CreatedAt` on (UserId, CreatedAt DESC)

---

### 3.18 UserPreferences

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| UserId | UNIQUEIDENTIFIER | FK → Users.Id, NOT NULL, UNIQUE | |
| Language | NVARCHAR(10) | NOT NULL, DEFAULT 'en' | |
| Currency | NVARCHAR(3) | NOT NULL, DEFAULT 'USD' | |
| Interests | NVARCHAR(MAX) | NULL | JSON array of interest IDs |
| IsDarkMode | BIT | NOT NULL, DEFAULT 0 | |
| IsAmoledMode | BIT | NOT NULL, DEFAULT 0 | |
| NotificationsEnabled | BIT | NOT NULL, DEFAULT 1 | |
| MarketingEmails | BIT | NOT NULL, DEFAULT 0 | |
| CreatedAt | DATETIME2 | NOT NULL | |
| UpdatedAt | DATETIME2 | NOT NULL | |

---

### 3.19 SubCategories

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| CategoryId | UNIQUEIDENTIFIER | FK → Categories.Id, NOT NULL | |
| Name | NVARCHAR(100) | NOT NULL | e.g., "Dentist", "Dermatologist" under "Doctors" |
| Slug | NVARCHAR(150) | NOT NULL | |
| IsActive | BIT | NOT NULL, DEFAULT 1 | |

---

### 3.20 BusinessSearchCache (Optional — for optimized search)

| Column | Type | Constraints | Notes |
|---|---|---|---|
| Id | UNIQUEIDENTIFIER | PK | |
| SearchTerm | NVARCHAR(500) | NOT NULL | |
| Location_Latitude | FLOAT | NOT NULL | |
| Location_Longitude | FLOAT | NOT NULL | |
| RadiusKm | INT | NOT NULL | |
| ResultData | NVARCHAR(MAX) | NOT NULL | JSON-cached results |
| CachedAt | DATETIME2 | NOT NULL | |
| ExpiresAt | DATETIME2 | NOT NULL | |

---

## 4. Seed Data

### 4.1 Categories & SubCategories

| Category | SubCategories |
|---|---|
| Doctors | Dentist, Dermatologist, Cardiologist, Ophthalmologist, Pediatrician, General Practitioner |
| Salons | Hair Styling, Nail Art, Barber, Makeup, Hair Coloring |
| Spas | Massage, Facial, Body Treatment, Aromatherapy, Hydrotherapy |
| Gyms | Personal Training, Yoga, Pilates, CrossFit, Zumba |
| Dining | Fine Dining, Casual, Brunch, Dinner, Reservations |
| Hotels | Luxury, Boutique, Business, Resort, Budget |

### 4.2 Admin User

- Email: `admin@bookify.com`
- Role: Admin (3)
- Seeded with a known password hash

### 4.3 Currencies & Languages (for personalization)

- Currencies: USD, AED, EUR, GBP, SAR, JPY, INR, CNY, BRL, AUD
- Languages: English, Arabic, French, Spanish, German, Portuguese, Japanese, Chinese, Hindi, Russian

---

## 5. Migration Strategy

- EF Core Code-First Migrations
- Each migration is a single atomic change
- Seed data in `DbContext.OnModelCreating()` or via migration SQL scripts
- Separate SQL scripts in `database/` folder for manual review
- Migration naming convention: `YYYYMMDD_HHMMSS_Description`

---

## 6. SQL Scripts

Separate `.sql` files will be generated in `database/` for:
- Full schema creation script
- Seed data scripts
- Index creation scripts (for advanced indexes not supported by EF Core conventions)
- Stored procedure scripts (if needed for reporting)

# Bookify — API Specification

## 1. API Design Conventions

- **Base URL:** `/api/v{version}`
- **Versioning:** URL path versioning (`/api/v1/`)
- **Content-Type:** `application/json` (request & response)
- **Authentication:** Bearer JWT token in `Authorization` header
- **Pagination:** Standard `page`, `pageSize` query params
- **Sorting:** `sortBy` (field name) + `sortDirection` (asc/desc)
- **Filtering:** `filter[fieldName]=value` convention
- **Searching:** `search` query param for full-text search
- **Consistent Response Envelope:**

```json
{
  "data": { ... },
  "success": true,
  "message": "Operation completed successfully",
  "errors": null
}
```

- **Error Response (ProblemDetails):**

```json
{
  "type": "https://httpstatuses.com/422",
  "title": "Validation Failed",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/appointments",
  "errors": {
    "ServiceId": ["'Service Id' must not be empty."]
  }
}
```

---

## 2. Authentication Endpoints

### `POST /api/v1/auth/register`

Register a new customer account.

**Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "phoneNumber": "+1234567890",
  "password": "SecureP@ss1",
  "confirmPassword": "SecureP@ss1"
}
```

**Response (201):**
```json
{
  "data": {
    "userId": "guid",
    "email": "john@example.com",
    "accessToken": "jwt...",
    "refreshToken": "rt...",
    "expiresIn": 900
  },
  "success": true
}
```

### `POST /api/v1/auth/login`

**Request:**
```json
{
  "email": "john@example.com",
  "password": "SecureP@ss1"
}
```

**Response (200):**
```json
{
  "data": {
    "accessToken": "jwt...",
    "refreshToken": "rt...",
    "expiresIn": 900
  },
  "success": true
}
```

### `POST /api/v1/auth/refresh`

**Request:**
```json
{
  "accessToken": "jwt...",
  "refreshToken": "rt..."
}
```

**Response (200):** Same as login.

### `POST /api/v1/auth/logout`

**Header:** `Authorization: Bearer jwt...`

**Request:**
```json
{
  "refreshToken": "rt..."
}
```

### `POST /api/v1/auth/forgot-password`

**Request:**
```json
{
  "email": "john@example.com"
}
```

### `POST /api/v1/auth/reset-password`

**Request:**
```json
{
  "email": "john@example.com",
  "token": "reset-token",
  "newPassword": "NewP@ss1"
}
```

---

## 3. User Endpoints

### `GET /api/v1/users/me`

Get current authenticated user's profile.

**Response (200):**
```json
{
  "data": {
    "id": "guid",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "phoneNumber": "+1234567890",
    "avatarUrl": "https://cdn.bookify.com/avatars/...",
    "role": "Customer",
    "isBiometricEnabled": false,
    "preferredLanguage": "en",
    "preferredCurrency": "USD"
  }
}
```

### `PUT /api/v1/users/me`

Update profile.

### `PUT /api/v1/users/me/password`

Change password.

### `DELETE /api/v1/users/me`

Delete account (soft delete).

---

## 4. Business Endpoints

### `GET /api/v1/businesses`

Search businesses with filtering, sorting, and pagination.

**Query Params:**
- `search` — Full-text search on name/description
- `category` — Filter by category slug
- `latitude`, `longitude`, `radiusKm` — Geo-location filter
- `ratingMin` — Minimum average rating (1-5)
- `priceMin`, `priceMax` — Price range
- `isVerified` — Only verified businesses
- `page`, `pageSize` — Pagination (default: 1, 20)
- `sortBy` — `rating`, `distance`, `name`, `createdAt`
- `sortDirection` — `asc`, `desc`

**Response (200):**
```json
{
  "data": [
    {
      "id": "guid",
      "name": "Lumina Health & Wellness",
      "slug": "lumina-health-wellness",
      "description": "Providing world-class medical consultation...",
      "category": "Doctors",
      "averageRating": 4.9,
      "totalReviews": 124,
      "priceLevel": "$$$",
      "coverImageUrl": "https://cdn.bookify.com/...",
      "city": "New York",
      "country": "USA",
      "distanceKm": 0.8,
      "isVerified": true,
      "isOpenNow": true
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 148,
    "totalPages": 8
  },
  "success": true
}
```

### `GET /api/v1/businesses/{slug}`

Get detailed business profile.

**Response (200):**
```json
{
  "data": {
    "id": "guid",
    "name": "Lumina Health & Wellness",
    "slug": "lumina-health-wellness",
    "description": "...",
    "email": "contact@lumina.com",
    "phoneNumber": "+1234567890",
    "address": {
      "line1": "221B Avenue of the Americas",
      "line2": "Suite 400",
      "city": "New York",
      "state": "NY",
      "postalCode": "10020",
      "country": "USA"
    },
    "geoLocation": {
      "latitude": 40.7580,
      "longitude": -73.9855
    },
    "website": "https://lumina.com",
    "isVerified": true,
    "bookingType": "Instant",
    "cancellationPolicy": "Free cancellation up to 24 hours before...",
    "timeZone": "America/New_York",
    "currency": "USD",
    "averageRating": 4.9,
    "totalReviews": 124,
    "categories": ["Doctors", "Wellness"],
    "coverImageUrl": "...",
    "logoUrl": "...",
    "gallery": [
      { "url": "...", "altText": "...", "isCover": false }
    ],
    "providers": [
      {
        "id": "guid",
        "userId": "guid",
        "firstName": "Elena",
        "lastName": "Vance",
        "title": "Director of Lumina Health",
        "bio": "...",
        "avatarUrl": "...",
        "averageRating": 4.9,
        "totalReviews": 240
      }
    ],
    "services": [
      {
        "id": "guid",
        "name": "General Health Checkup",
        "description": "...",
        "durationMinutes": 45,
        "price": 120.00,
        "currency": "USD",
        "category": "Medical Consultations"
      }
    ],
    "openingHours": {
      "monday": { "open": "09:00", "close": "18:00" },
      "tuesday": { "open": "09:00", "close": "20:00" }
    }
  }
}
```

### `GET /api/v1/businesses/{slug}/reviews`

Paginated reviews for a business.

### `GET /api/v1/businesses/{slug}/availability`

Get available time slots for a provider/date.

**Query Params:**
- `providerId` (optional) — Filter by specific provider
- `date` — Date to check (YYYY-MM-DD)
- `serviceId` — Service (for duration calculation)

### `POST /api/v1/businesses` (BusinessOwner only)

Create a new business listing.

### `PUT /api/v1/businesses/{id}` (Owner only)

Update business details.

---

## 5. Category Endpoints

### `GET /api/v1/categories`

List all categories with subcategories.

**Response (200):**
```json
{
  "data": [
    {
      "id": "guid",
      "name": "Doctors",
      "slug": "doctors",
      "iconName": "medical_services",
      "displayOrder": 1,
      "subcategories": [
        { "id": "guid", "name": "Dentist", "slug": "dentist" },
        { "id": "guid", "name": "Dermatologist", "slug": "dermatologist" }
      ]
    }
  ]
}
```

---

## 6. Provider Endpoints

### `GET /api/v1/providers/{id}`

Get provider details.

### `GET /api/v1/providers/{id}/availability`

Get weekly availability schedule + date overrides.

### `PUT /api/v1/providers/{id}/availability`

Update availability schedule (Provider/BusinessOwner only).

---

## 7. Service Endpoints

### `GET /api/v1/businesses/{slug}/services`

List services for a business.

---

## 8. Appointment Endpoints

### `POST /api/v1/appointments`

Create a new appointment.

**Request:**
```json
{
  "providerId": "guid",
  "serviceId": "guid",
  "businessId": "guid",
  "startTime": "2023-10-14T14:00:00Z",
  "endTime": "2023-10-14T15:30:00Z",
  "customerNotes": "First visit, please arrive 10 minutes early"
}
```

**Response (201):**
```json
{
  "data": {
    "id": "guid",
    "bookingReference": "BOK-7F3A2K",
    "status": "Pending",
    "startTime": "2023-10-14T14:00:00Z",
    "endTime": "2023-10-14T15:30:00Z",
    "totalAmount": 120.00,
    "currency": "USD",
    "serviceName": "General Health Checkup",
    "providerName": "Elena Vance",
    "businessName": "Lumina Health & Wellness",
    "businessAddress": "221B Avenue of the Americas..."
  }
}
```

### `GET /api/v1/appointments`

Get current user's appointments (as customer or provider).

**Query Params:**
- `status` — Filter by status
- `role` — `customer` (default) or `provider`
- `from`, `to` — Date range filter
- `page`, `pageSize` — Pagination

### `GET /api/v1/appointments/{id}`

Get appointment details.

### `PUT /api/v1/appointments/{id}/cancel`

Cancel an appointment.

**Request:**
```json
{
  "reason": "Schedule conflict"
}
```

### `PUT /api/v1/appointments/{id}/reschedule`

Reschedule an appointment.

**Request:**
```json
{
  "newStartTime": "2023-10-16T10:00:00Z",
  "newEndTime": "2023-10-16T11:30:00Z"
}
```

### `PUT /api/v1/appointments/{id}/confirm` (Provider/Owner)

Confirm a pending appointment.

### `PUT /api/v1/appointments/{id}/complete`

Mark appointment as completed.

---

## 9. Review Endpoints

### `POST /api/v1/appointments/{id}/review`

Submit a review for a completed appointment.

**Request:**
```json
{
  "rating": 5,
  "comment": "Excellent service! Very professional and caring."
}
```

### `GET /api/v1/reviews?businessId={id}`

Get reviews for a business.

### `PUT /api/v1/reviews/{id}`

Update own review.

### `DELETE /api/v1/reviews/{id}`

Delete own review.

---

## 10. Payment Endpoints

### `POST /api/v1/payments/initialize`

Initialize a payment for an appointment.

**Request:**
```json
{
  "appointmentId": "guid",
  "paymentMethod": "CreditCard",
  "returnUrl": "https://bookify.com/booking/confirmation"
}
```

### `POST /api/v1/payments/{id}/confirm`

Confirm payment after PSP callback.

### `GET /api/v1/payments/{id}`

Get payment status.

---

## 11. Notification Endpoints

### `GET /api/v1/notifications`

Get user's notifications (paginated, sorted by newest).

### `PUT /api/v1/notifications/{id}/read`

Mark notification as read.

### `PUT /api/v1/notifications/read-all`

Mark all notifications as read.

### `DELETE /api/v1/notifications/{id}`

Delete a notification.

---

## 12. Dashboard Endpoints

### `GET /api/v1/dashboard/summary`

Customer dashboard summary.

**Response:**
```json
{
  "upcomingAppointments": 2,
  "pastAppointments": 15,
  "totalSpent": 1840.00,
  "currency": "USD",
  "unreadNotifications": 3
}
```

### `GET /api/v1/dashboard/upcoming`

Upcoming appointments list.

### `GET /api/v1/dashboard/history`

Past appointments history.

---

## 13. Business Dashboard Endpoints (Owner)

### `GET /api/v1/dashboard/business/{businessId}/summary`

Owner dashboard with revenue, booking counts, etc.

### `GET /api/v1/dashboard/business/{businessId}/analytics`

Revenue over time, popular services, peak hours.

---

## 14. Settings Endpoints

### `GET /api/v1/settings/preferences`

Get user preferences.

### `PUT /api/v1/settings/preferences`

Update user preferences.

**Request:**
```json
{
  "language": "en",
  "currency": "USD",
  "interests": ["health", "beauty", "fitness"],
  "isDarkMode": false,
  "isAmoledMode": false,
  "notificationsEnabled": true,
  "marketingEmails": false
}
```

### `PUT /api/v1/settings/biometric`

Toggle biometric authentication.

---

## 15. Search Endpoints

### `GET /api/v1/search/ai`

AI-powered search with natural language understanding.

**Query Params:**
- `query` — Natural language query (e.g., "Find a good dentist near me open on weekends")
- `latitude`, `longitude` — User's location
- `filters` — JSON object with advanced filters

**Response (200):**
```json
{
  "data": {
    "aiInterpretation": {
      "intent": "Find Dentist",
      "extractedFilters": {
        "category": "Dentist",
        "location": "near me",
        "openOn": "Saturday"
      }
    },
    "results": [ ... ],
    "suggestedSearches": ["Pediatric dentist", "Teeth whitening specials"]
  }
}
```

---

## 16. Health Endpoints

### `GET /health`

Simple liveness check.

### `GET /health/ready`

Readiness check (DB + Redis + Blob).

### `GET /health/startup`

Startup check.

---

## 17. Admin Endpoints (Admin role only)

- `GET /api/v1/admin/users` — List all users
- `PUT /api/v1/admin/users/{id}/role` — Change user role
- `DELETE /api/v1/admin/users/{id}` — Force delete user
- `GET /api/v1/admin/businesses` — All businesses
- `PUT /api/v1/admin/businesses/{id}/verify` — Verify business
- `GET /api/v1/admin/analytics` — Platform-level analytics

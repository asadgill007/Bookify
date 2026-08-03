# Google Sign-In (Customer/User accounts)

Google Sign-In is fully implemented end-to-end. This document explains how it
works, the exact configuration it needs, and how to run and test it.

## Flow

```
User taps "Sign in with Google"  (Flutter)
  → google_sign_in returns a Google ID token
  → Flutter sends { idToken, accountType } to  POST /api/v1/auth/google
  → Backend validates the ID token against Google's public JWKS:
      signature, issuer (https://accounts.google.com), audience (Google:ClientId),
      lifetime/expiry, email claim and email_verified
  → User lookup by Google subject → fallback by normalized email
  → No user   → creates a Customer account (or the accountType passed), links Google identity
  → User found → signs in; if the email matched a local account, the Google identity is linked
  → Issues the standard Bookify JWT access token + refresh token
  → Tokens stored in flutter_secure_storage; role-based navigation
```

Google is **only the identity provider**. Bookify remains the authority for the
access token, refresh token, roles and authorization. Identity never comes from
the client — it comes from the validated Google token.

## Configuration required

### 1. Backend — `Google:ClientId`

The backend validates the Google token's **audience** against this value. It is
read from `Google:ClientId` (appsettings / environment variable / user secrets).
Use the **web client ID** of the existing Google Cloud OAuth app (e.g.
`....apps.googleusercontent.com`). Set it in development:

```powershell
cd backend/src/Bookify.WebApi
dotnet user-secrets set "Google:ClientId" "<WEB_CLIENT_ID>"
```

> When `Google:ClientId` is empty the validator skips strict audience matching
> (development fallback only). Set it for real verification.

No secret value lives in source control; `appsettings.json` keeps it empty.

### 2. Flutter Web — OAuth client ID

`google_sign_in` on the web requires a client ID. It is injected at build/run
time via a dart-define — never hardcoded:

```bash
cd mobile
flutter run -d edge --dart-define=GOOGLE_CLIENT_ID=<WEB_CLIENT_ID>
```

**Authorized JavaScript origin** must match the *actual* URL Flutter serves on.
`flutter run -d edge` picks a random local port, so add that exact origin to the
OAuth client's "Authorized JavaScript origins" in Google Cloud Console (e.g.
`http://localhost:53601`). If the origin is not authorized, the Google popup
fails with `redirect_uri_mismatch` / `origin_mismatch`.

### 3. Android

- `applicationId` / namespace: **`com.bookify.bookify`** (`mobile/android/app/build.gradle.kts`).
- The Google Cloud Console **Android OAuth client** for this package must
  register the app's **SHA-1 signing certificate**. The local debug SHA-1
  (from `~/.android/debug.keystore`) is:

  ```
  12:95:7D:82:E3:3F:2C:F2:4A:AB:53:9D:36:9A:9D:3B:CB:16:AE:9D
  ```

  (Regenerate it with `keytool -list -v -keystore ~/.android/debug.keystore
  -alias androiddebugkey -storepass android` if your keystore differs.)
- For the plugin to return an **ID token** on Android it needs the server
  (web) client ID, passed the same way:

  ```bash
  flutter run --dart-define=GOOGLE_CLIENT_ID=<WEB_CLIENT_ID>
  ```

  Without a client ID, sign-in still works but `getIdToken()` returns null and
  the app reports that it could not get an ID token.

## Files involved

Backend:
- `Bookify.WebApi/Controllers/v1/AuthController.cs` — `POST /auth/google`
- `Bookify.Application/Commands/Auth/GoogleLoginCommand.cs` — command + validation
- `Bookify.Infrastructure/Authentication/GoogleIdTokenValidator.cs` — JWKS validation
- `Bookify.Infrastructure/Services/AuthService.cs` — `LoginWithGoogleAsync` (create / link / sign-in)
- `Bookify.Domain/Entities/User.cs` — `GoogleSubject`, `GoogleName`, `GooglePictureUrl`, `LinkGoogleAccount`
- Migration `20260801015218_AddFavoritesChatSupportGoogle` (already applied)

Flutter:
- `core/services/google_sign_in_service.dart` — `GoogleSignIn` wrapper (lazy, guarded)
- `core/services/social_auth_service.dart`
- `features/auth/widgets/google_sign_in_button.dart` — button with idle/loading states
- `features/auth/providers/auth_provider.dart` — `googleSignIn(...)` state + error surfacing
- `features/auth/screens/login_screen.dart` / `register_screen.dart` — button integration

## Security notes

- ID tokens are validated server-side (signature, issuer, audience, expiry, `email_verified`); unverified emails are rejected with `EMAIL_NOT_VERIFIED`.
- No OAuth client secret exists anywhere in the Flutter app; only the public web client ID is injected at build time.
- Access + refresh tokens are stored in `flutter_secure_storage`.
- No duplicate accounts: lookup by Google subject first, then by email; an existing local account is linked, never duplicated.
- Google users get a random unguessable password hash so password login is not silently possible.

## Tests

- `backend/tests/Bookify.Application.Tests/GoogleLoginCommandValidatorTests.cs` — request validation
- `backend/tests/Bookify.Infrastructure.Tests/AuthServiceGoogleTests.cs` — invalid token, unverified email, new user, existing Google user, local-account linking, JWT + refresh issuance
- `mobile/test/auth_google_test.dart` — Flutter `googleSignIn` success / backend-error / network-error states

Run everything:

```powershell
cd backend && dotnet build Bookify.slnx && dotnet test Bookify.slnx
cd mobile  && flutter pub get && flutter analyze && flutter test
```

## Manual test checklist (Microsoft Edge)

1. Start the API: `backend/start-api.ps1` (serves on `http://localhost:5136`).
2. `cd mobile && flutter run -d edge --dart-define=GOOGLE_CLIENT_ID=<WEB_CLIENT_ID>`
3. Confirm the API's CORS policy allows the Flutter origin (development allows any origin).
4. Verify: login page → "Sign in with Google" → account picker → test-user login → new account created → logout → login again (existing user) → protected route works → refresh-token flow (access token renews after 15 min without re-login).

# Bookify Mobile App

Premium Flutter appointment booking application with glassmorphism design.

## Prerequisites

- Flutter SDK (3.24.0 or higher)
- Android SDK with NDK installed
- Physical Android device connected via USB with debugging enabled
- Backend API running (default: http://localhost:5136)

## Quick Start

### 1. Start the Backend API

```powershell
# From the project root
powershell -ExecutionPolicy Bypass -File ..\backend\start-api.ps1
```

The API will be available at `http://localhost:5136`.

### 2. Clean Android Build (if needed)

If you encounter build errors related to NDK or Gradle daemons:

```powershell
# From the mobile directory
powershell -ExecutionPolicy Bypass -File .\clean-android-build.ps1
```

This script will:
- Stop all Gradle daemons
- Kill lingering Java processes
- Clear Gradle caches
- Verify NDK installation
- Clean Flutter build artifacts

### 3. Install Dependencies

```powershell
cd d:\Bookify\mobile
flutter pub get
```

### 4. Run on Physical Device

```powershell
# List connected devices
flutter devices

# Run on your device (replace <device-id> with your actual device ID)
flutter run -d <device-id> --dart-define=API_BASE_URL=http://192.168.100.74:5136
```

**Important:** The `--dart-define=API_BASE_URL` flag is required to connect to the backend API from a physical device. Use your machine's local network IP address (not localhost).

## Project Structure

```
mobile/
├── lib/
│   ├── core/
│   │   ├── theme/          # App theme, glassmorphism components
│   │   ├── network/        # API client
│   │   └── constants/      # API endpoints
│   └── features/
│       ├── onboarding/     # Welcome screens
│       ├── auth/           # Login, register
│       ├── home/           # Discovery, search
│       ├── business/       # Business profiles
│       ├── appointments/   # Booking, checkout, confirmation
│       ├── settings/       # App settings
│       └── profile/        # User profile
├── android/                # Android native code
├── clean-android-build.ps1 # Build cleanup script
└── README.md
```

## Troubleshooting

### NDK Corruption Error

**Error:** "This is likely due to a malformed download of the NDK"

**Solution:**
1. Run the cleanup script: `powershell -ExecutionPolicy Bypass -File .\clean-android-build.ps1`
2. If the issue persists, delete the NDK folder manually:
   ```
   C:\Users\user\AppData\Local\Android\sdk\ndk\<version>
   ```
3. Reinstall via Android Studio SDK Manager

### Gradle Daemon Port Conflict

**Error:** "Could not create service of type FileLockContentionHandler... Address already in use"

**Solution:**
1. Run the cleanup script - it will stop all daemons and kill Java processes
2. Or manually stop daemons:
   ```powershell
   cd d:\Bookify\mobile\android
   ./gradlew --stop
   ```

### Build Timeout

First-time NDK downloads can take 15-20 minutes. If the build times out:
1. Run `flutter clean`
2. Ensure you have a stable internet connection
3. Run the build again without timeout restrictions

## Build & Release

### Debug Build

```powershell
flutter run -d <device-id> --dart-define=API_BASE_URL=http://192.168.100.74:5136
```

### Release Build

```powershell
flutter build apk --release --dart-define=API_BASE_URL=http://192.168.100.74:5136
```

The release APK will be generated at: `build\app\outputs\flutter-apk\app-release.apk`

## Design System

The app uses a premium glassmorphism design system:
- **Colors:** Indigo luxury gradient accents
- **Typography:** Clear hierarchy with bold headings
- **Components:** GlassContainer, GradientBackground
- **Dark Mode:** Full support with toggle in Settings
- **Animations:** Smooth transitions via flutter_animate

## Backend API

The app requires the Bookify backend API running. See the main project README for backend setup instructions.

Default API endpoints:
- Categories: `GET /api/v1/categories`
- Businesses: `GET /api/v1/businesses`
- Reviews: `GET /api/v1/reviews`

## License

Proprietary - All rights reserved
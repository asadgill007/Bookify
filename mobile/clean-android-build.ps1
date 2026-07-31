#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Cleans Android build artifacts, stops Gradle daemons, and fixes NDK issues.
.DESCRIPTION
    This script resolves common Android build failures:
    1. Stops all Gradle daemons
    2. Kills lingering Java processes holding ports/locks
    3. Clears Gradle caches and build artifacts
    4. Optionally cleans corrupted NDK installation
    5. Verifies NDK version matches build.gradle.kts
#>

$ErrorActionPreference = 'Stop'
$PROJECT_ROOT = "d:\Bookify\mobile"
$ANDROID_DIR = "$PROJECT_ROOT\android"
$NDK_DIR = "$env:LOCALAPPDATA\Android\sdk\ndk"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Bookify Android Build Cleaner" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Stop Gradle daemons
Write-Host "[1/5] Stopping Gradle daemons..." -ForegroundColor Yellow
Push-Location $ANDROID_DIR
try {
    & ./gradlew --stop 2>&1 | Out-Null
    Write-Host "      Gradle daemons stopped" -ForegroundColor Green
} catch {
    Write-Host "      Warning: Could not stop Gradle daemons: $_" -ForegroundColor Yellow
}
Pop-Location

# Step 2: Kill lingering Java processes
Write-Host "`n[2/5] Killing lingering Java processes..." -ForegroundColor Yellow
$javaProcesses = Get-Process -Name "java" -ErrorAction SilentlyContinue
if ($javaProcesses) {
    foreach ($proc in $javaProcesses) {
        Write-Host "      Killing Java process $($proc.Id)..." -ForegroundColor Yellow
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 2
    Write-Host "      Java processes killed" -ForegroundColor Green
} else {
    Write-Host "      No lingering Java processes found" -ForegroundColor Green
}

# Step 3: Clear Gradle caches
Write-Host "`n[3/5] Clearing Gradle caches..." -ForegroundColor Yellow
$gradleCachePaths = @(
    "$env:USERPROFILE\.gradle\caches",
    "$ANDROID_DIR\.gradle",
    "$ANDROID_DIR\build",
    "$PROJECT_ROOT\build"
)

foreach ($path in $gradleCachePaths) {
    if (Test-Path $path) {
        Write-Host "      Removing: $path" -ForegroundColor DarkGray
        Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
    }
}
Write-Host "      Gradle caches cleared" -ForegroundColor Green

# Step 4: Check NDK installation
Write-Host "`n[4/5] Checking NDK installation..." -ForegroundColor Yellow

# Read expected NDK version from build.gradle.kts
$buildGradlePath = "$ANDROID_DIR\app\build.gradle.kts"
$expectedNdkVersion = $null

if (Test-Path $buildGradlePath) {
    $buildContent = Get-Content $buildGradlePath -Raw
    if ($buildContent -match 'ndkVersion\s*=\s*"([^"]+)"') {
        $expectedNdkVersion = $Matches[1]
        Write-Host "      Expected NDK version: $expectedNdkVersion" -ForegroundColor Cyan
    }
}

# Check installed NDK versions
if (Test-Path $NDK_DIR) {
    $installedNdks = Get-ChildItem $NDK_DIR -Directory | Select-Object -ExpandProperty Name
    Write-Host "      Installed NDK versions: $($installedNdks -join ', ')" -ForegroundColor Cyan
    
    # Check if expected version exists
    if ($expectedNdkVersion -and ($installedNdks -contains $expectedNdkVersion)) {
        $ndkPath = "$NDK_DIR\$expectedNdkVersion"
        $sourceProperties = "$ndkPath\source.properties"
        
        if (Test-Path $sourceProperties) {
            Write-Host "      NDK $expectedNdkVersion appears valid" -ForegroundColor Green
        } else {
            Write-Host "      WARNING: NDK $expectedNdkVersion may be corrupted (missing source.properties)" -ForegroundColor Yellow
            Write-Host "      Consider reinstalling NDK via Android Studio SDK Manager" -ForegroundColor Yellow
        }
    } elseif ($expectedNdkVersion) {
        Write-Host "      WARNING: Expected NDK $expectedNdkVersion not found" -ForegroundColor Yellow
        $installCmd = "sdkmanager --install ndk;$expectedNdkVersion"
        Write-Host "      Install it via: $installCmd" -ForegroundColor Yellow
    }
} else {
    Write-Host "      WARNING: NDK directory not found at $NDK_DIR" -ForegroundColor Yellow
}

# Step 5: Clean Flutter build
Write-Host "`n[5/5] Cleaning Flutter build artifacts..." -ForegroundColor Yellow
Push-Location $PROJECT_ROOT
try {
    & flutter clean 2>&1 | Out-Null
    Write-Host "      Flutter build cleaned" -ForegroundColor Green
} catch {
    Write-Host "      Warning: flutter clean failed: $_" -ForegroundColor Yellow
}
Pop-Location

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  Android build environment cleaned" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Run: cd $PROJECT_ROOT" -ForegroundColor White
Write-Host "  2. Run: flutter pub get" -ForegroundColor White
Write-Host "  3. Run: flutter run -d <device-id> --dart-define=API_BASE_URL=http://192.168.100.74:5136" -ForegroundColor White

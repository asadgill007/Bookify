#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Reliably starts the Bookify API on a fixed port with health check.
.DESCRIPTION
    This script:
    1. Kills any existing process on the configured port
    2. Cleans stale build locks
    3. Builds the project
    4. Starts the API in the background
    5. Waits for health endpoint to return healthy
    6. Prints the API URL
#>

$ErrorActionPreference = 'Stop'
$PORT = 5136
$API_URL = "http://localhost:$PORT"
$PROJECT_PATH = "d:\Bookify\backend\src\Bookify.WebApi\Bookify.WebApi.csproj"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Bookify API Startup Script" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Step 1: Kill existing process on port
Write-Host "[1/5] Checking for existing process on port $PORT..." -ForegroundColor Yellow
$existingProcess = netstat -ano | Select-String ":$PORT\s" | Select-String "LISTENING"
if ($existingProcess) {
    $processId = ($existingProcess -split '\s+')[-1]
    Write-Host "      Found process $processId, killing..." -ForegroundColor Yellow
    Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    Write-Host "      ✓ Process killed" -ForegroundColor Green
} else {
    Write-Host "      ✓ No existing process found" -ForegroundColor Green
}

# Step 2: Clean build locks
Write-Host "`n[2/5] Cleaning build artifacts..." -ForegroundColor Yellow
$lockFiles = @(
    "d:\Bookify\backend\src\Bookify.WebApi\obj\project.assets.json",
    "d:\Bookify\backend\src\Bookify.WebApi\obj\Bookify.WebApi.csproj.nuget.dgspec.json"
)
foreach ($lock in $lockFiles) {
    if (Test-Path $lock) {
        Remove-Item $lock -Force -ErrorAction SilentlyContinue
    }
}
Write-Host "      ✓ Build artifacts cleaned" -ForegroundColor Green

# Step 3: Build project
Write-Host "`n[3/5] Building project..." -ForegroundColor Yellow
$buildOutput = dotnet build $PROJECT_PATH --no-restore 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "      ✗ Build failed:" -ForegroundColor Red
    Write-Host $buildOutput
    exit 1
}
Write-Host "      ✓ Build successful" -ForegroundColor Green

# Step 4: Start API in background
Write-Host "`n[4/5] Starting API in background..." -ForegroundColor Yellow
$process = Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$PROJECT_PATH`"" -PassThru -WindowStyle Hidden
$processId = $process.Id
Write-Host "      API started (PID: $($process.Id))" -ForegroundColor Green

# Step 5: Wait for health check
Write-Host "`n[5/5] Waiting for API to be healthy..." -ForegroundColor Yellow
$maxRetries = 30
$retryCount = 0
$healthUrl = "$API_URL/health"
$healthy = $false

while ($retryCount -lt $maxRetries -and -not $healthy) {
    try {
        $response = Invoke-WebRequest -Uri $healthUrl -Method Get -UseBasicParsing -TimeoutSec 2
        if ($response.StatusCode -eq 200) {
            $healthy = $true
            break
        }
    } catch {
        # Not ready yet
    }
    Write-Host "      Waiting... ($($retryCount + 1)/$maxRetries)" -ForegroundColor DarkGray
    Start-Sleep -Seconds 2
    $retryCount++
}

if (-not $healthy) {
    Write-Host "`n      ✗ API failed to start within timeout" -ForegroundColor Red
    Write-Host "      Check logs with: dotnet run --project `"$PROJECT_PATH`"" -ForegroundColor Yellow
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  API is running and healthy" -ForegroundColor Green
Write-Host "  URL: $API_URL" -ForegroundColor Cyan
Write-Host "  PID: $($process.Id)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Green

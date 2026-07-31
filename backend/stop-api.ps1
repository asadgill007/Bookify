<#
.SYNOPSIS
    Stop the Bookify API process.

.DESCRIPTION
    Kills any dotnet process bound to port 5136 and cleans up VBCSCompiler.
    Usage: .\stop-api.ps1
#>

$Port = 5136

Write-Host "🛑 Stopping Bookify API on port $Port..." -ForegroundColor Yellow

# Kill process on port
$processOnPort = netstat -ano | findstr ":$Port "
if ($processOnPort) {
    $pidOnPort = ($processOnPort | Select-String "LISTENING" | ForEach-Object { $_ -replace '.*\s+(\d+)\s*$', '$1' } | Select-Object -First 1)
    if ($pidOnPort -and $pidOnPort -match '^\d+$') {
        taskkill /F /PID $pidOnPort 2>$null
        Write-Host "   ✅ API process (PID $pidOnPort) stopped." -ForegroundColor Green
    }
} else {
    Write-Host "   No API process found on port $Port." -ForegroundColor Cyan
}

# Kill VBCSCompiler
$vbCsProc = Get-Process -Name "VBCSCompiler" -ErrorAction SilentlyContinue
if ($vbCsProc) {
    Stop-Process -Name "VBCSCompiler" -Force -ErrorAction SilentlyContinue
    Write-Host "   ✅ VBCSCompiler cleaned up." -ForegroundColor Green
}

Write-Host ""
Write-Host "✅ API stopped." -ForegroundColor Green

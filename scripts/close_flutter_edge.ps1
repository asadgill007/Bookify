# Closes msedge instances that were launched by `flutter run` (their command
# line references the flutter_tools user-data-dir), without touching other
# user Edge windows.
$procs = Get-CimInstance Win32_Process -Filter "Name='msedge.exe'"
foreach ($p in $procs) {
    if ($p.CommandLine -match 'flutter_tools') {
        Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue
        Write-Host "closed edge pid $($p.ProcessId)"
    }
}

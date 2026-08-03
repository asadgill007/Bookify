# Locates the Edge instance launched by `flutter run` (command line contains
# flutter_tools_chrome_device) and prints its PID and listening TCP ports.
$edges = Get-CimInstance Win32_Process -Filter "Name='msedge.exe'" |
    Where-Object { $_.CommandLine -match 'flutter_tools_chrome_device' }

foreach ($e in $edges) {
    Write-Host "EDGE PID=$($e.ProcessId)"
    $listening = Get-NetTCPConnection -State Listen -OwningProcess $e.ProcessId -ErrorAction SilentlyContinue
    foreach ($l in $listening) {
        Write-Host "  LISTEN $($l.LocalAddress):$($l.LocalPort)"
    }
    # Edge's browser process is the parent; also check all its children's ports.
    $children = Get-CimInstance Win32_Process | Where-Object { $_.ParentProcessId -eq $e.ProcessId }
    foreach ($c in $children) {
        $ports = Get-NetTCPConnection -State Listen -OwningProcess $c.ProcessId -ErrorAction SilentlyContinue
        foreach ($p in $ports) {
            Write-Host "  CHILD $($c.ProcessId) LISTEN $($p.LocalAddress):$($p.LocalPort)"
        }
    }
}

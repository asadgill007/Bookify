$procs = Get-CimInstance Win32_Process | Where-Object { $_.Name -match '^dart' }
foreach ($p in $procs) {
  $cmd = if ($p.CommandLine) { $p.CommandLine.Substring(0, [Math]::Min(160, $p.CommandLine.Length)) } else { '' }
  Write-Host "$($p.ProcessId) $($p.Name) $cmd"
}

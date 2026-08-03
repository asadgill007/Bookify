$paths = @(
  "$env:LOCALAPPDATA\Temp",
  "$env:LOCALAPPDATA\Pub",
  "$env:LOCALAPPDATA\Android",
  "$env:USERPROFILE\.gradle",
  "$env:LOCALAPPDATA\Google",
  "$env:APPDATA\npm-cache",
  "$env:LOCALAPPDATA\Microsoft\Windows\INetCache",
  "$env:LOCALAPPDATA\flutter_tools.*",
  "$env:USERPROFILE\.android",
  "$env:LOCALAPPDATA\go"
)
foreach ($p in $paths) {
  $items = Get-Item -Path $p -ErrorAction SilentlyContinue
  foreach ($it in $items) {
    $size = (Get-ChildItem -Path $it.FullName -Recurse -Force -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
    $mb = [Math]::Round($size / 1MB, 0)
    Write-Host ("{0} MB  {1}" -f $mb, $it.FullName)
  }
}

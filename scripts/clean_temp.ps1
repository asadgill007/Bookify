$temp = Join-Path $env:LOCALAPPDATA 'Temp'
$before = (Get-ChildItem -Path $temp -Force -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
Get-ChildItem -Path $temp -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
$after = (Get-ChildItem -Path $temp -Force -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
Write-Host ("freed MB: {0}" -f [Math]::Round(($before - $after) / 1MB, 0))
Write-Host ("remaining MB: {0}" -f [Math]::Round($after / 1MB, 0))

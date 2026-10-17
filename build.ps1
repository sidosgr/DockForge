param([switch]$SkipInstallers,[string]$InnoCompiler)
$ErrorActionPreference='Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$dotnet=Join-Path $PSScriptRoot '.tools\dotnet\dotnet.exe'
if (!(Test-Path -LiteralPath $dotnet)) { $dotnet=(Get-Command dotnet).Source }
Push-Location $PSScriptRoot
try {
 & $dotnet run --project tests/DockForge.Tests -c Release
 if ($LASTEXITCODE) { throw 'Core tests failed.' }
 foreach ($arch in @('x64','arm64')) {
  & $dotnet publish src/DockForge/DockForge.csproj -c Release -r "win-$arch" --self-contained true -p:DebugType=None -p:DebugSymbols=false -o "artifacts/publish/win-$arch"
  if ($LASTEXITCODE) { throw "Publish failed: $arch" }
 }
 if (!$SkipInstallers) {
  if (!$InnoCompiler) { $InnoCompiler=Join-Path $PSScriptRoot '.tools\inno\ISCC.exe' }
  if (!(Test-Path -LiteralPath $InnoCompiler)) { throw 'Set -InnoCompiler to the path of Inno Setup 6.7+ ISCC.exe.' }
  & $InnoCompiler /Qp installer/DockForge.iss
  if ($LASTEXITCODE) { throw 'Installer failed.' }
  Get-Item artifacts/installers/DockForge-Setup-0.1.4.exe | Get-FileHash -Algorithm SHA256 | ForEach-Object { "$($_.Hash.ToLower())  $(Split-Path $_.Path -Leaf)" } | Set-Content artifacts/installers/SHA256SUMS.txt
 }
} finally { Pop-Location }

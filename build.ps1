param([switch]$SkipInstallers,[string]$InnoCompiler,[switch]$SkipPublish,[switch]$RequireSignedPayload)
$ErrorActionPreference='Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$version=([xml](Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Directory.Build.props') -Raw)).Project.PropertyGroup.Version
if ($version -notmatch '^\d+\.\d+\.\d+$') { throw 'Expected a three-part release version.' }
$dotnet=Join-Path $PSScriptRoot '.tools\dotnet\dotnet.exe'
if (!(Test-Path -LiteralPath $dotnet)) { $dotnet=(Get-Command dotnet).Source }
Push-Location $PSScriptRoot
try {
 if (!$SkipPublish) {
  & $dotnet run --project tests/DockForge.Tests -c Release
  if ($LASTEXITCODE) { throw 'Core tests failed.' }
  foreach ($arch in @('x64','arm64')) {
   & $dotnet publish src/DockForge/DockForge.csproj -c Release -r "win-$arch" --self-contained true -p:DebugType=None -p:DebugSymbols=false -o "artifacts/publish/win-$arch"
   if ($LASTEXITCODE) { throw "Publish failed: $arch" }
  }
 }
 if ($RequireSignedPayload) {
  $ownedFiles=foreach ($arch in @('x64','arm64')) { foreach ($name in @('DockForge.exe','DockForge.dll','DockForge.Core.dll')) { Join-Path "artifacts/publish/win-$arch" $name } }
  & ./scripts/Verify-SignedArtifacts.ps1 -Path $ownedFiles
 }
 if (!$SkipInstallers) {
  if (!$InnoCompiler) { $InnoCompiler=Join-Path $PSScriptRoot '.tools\inno\ISCC.exe' }
  if (!(Test-Path -LiteralPath $InnoCompiler)) { throw 'Set -InnoCompiler to the path of Inno Setup 6.7+ ISCC.exe.' }
  & $InnoCompiler /Qp "/DAppVersion=$version" installer/DockForge.iss
  if ($LASTEXITCODE) { throw 'Installer failed.' }
  Get-Item -LiteralPath "artifacts/installers/DockForge-Setup-$version.exe" | Get-FileHash -Algorithm SHA256 | ForEach-Object { "$($_.Hash.ToLower())  $(Split-Path $_.Path -Leaf)" } | Set-Content artifacts/installers/SHA256SUMS.txt
 }
} finally { Pop-Location }

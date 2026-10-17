param(
 [Parameter(Mandatory)][string[]]$Path,
 [string]$Publisher = 'SignPath Foundation'
)
$ErrorActionPreference = 'Stop'
$signerThumbprint = $null
foreach ($file in $Path) {
 $resolved = (Resolve-Path -LiteralPath $file).Path
 $signature = Get-AuthenticodeSignature -LiteralPath $resolved
 if ($signature.Status -ne 'Valid') { throw "Invalid or missing Authenticode signature: $file ($($signature.Status))." }
 if (!$signature.TimeStamperCertificate) { throw "Missing trusted timestamp: $file." }
 $expectedSubject = '(^|,\s*)CN=' + [regex]::Escape($Publisher) + '(,|$)'
 if (!$signature.SignerCertificate -or $signature.SignerCertificate.Subject -notmatch $expectedSubject) { throw "Unexpected signing publisher: $file." }
 if ($signerThumbprint -and $signerThumbprint -ne $signature.SignerCertificate.Thumbprint) { throw 'The artifacts were signed by different certificates.' }
 $signerThumbprint = $signature.SignerCertificate.Thumbprint
 Write-Output "Verified signature and timestamp: $(Split-Path $resolved -Leaf)"
}
if (!$signerThumbprint) { throw 'No signed artifacts were verified.' }

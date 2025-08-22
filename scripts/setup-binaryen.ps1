param()
$ErrorActionPreference = 'Stop'

$ROOT = Split-Path -Parent $MyInvocation.MyCommand.Path | Split-Path -Parent
$VERSION = Get-Content "$ROOT/toolchain/binaryen.version" -Raw

switch ($env:OS) {
  'Windows_NT' { $OS = 'windows'; $LibVar = 'PATH' }
  default {
    Write-Error "Unsupported OS $env:OS"; exit 1 }
}
$ARCH = if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -eq [System.Runtime.InteropServices.Architecture]::X64) {
  'x86_64'
} elseif ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -eq [System.Runtime.InteropServices.Architecture]::Arm64) {
  'arm64'
} else { Write-Error "Unsupported architecture"; exit 1 }
$RID = "$OS-" + (if ($ARCH -eq 'x86_64') { 'x64' } else { 'arm64' })
$EXT = 'zip'
$File = "binaryen-$VERSION-$ARCH-$OS.$EXT"
$Url = "https://github.com/WebAssembly/binaryen/releases/download/$VERSION/$File"
$Dest = Join-Path $ROOT "toolchain/binaryen/$RID"
New-Item -ItemType Directory -Force -Path $Dest | Out-Null
$Archive = New-TemporaryFile
if (-not (Test-Path (Join-Path $Dest 'bin'))) {
  Invoke-WebRequest -Uri $Url -OutFile $Archive
  $shaUrl = "$Url.sha256"
  try {
    Invoke-WebRequest -Uri $shaUrl -OutFile "$Archive.sha256"
    $expected = Get-Content "$Archive.sha256" | ForEach-Object { $_.Split(' ')[0] }
    $actual = (Get-FileHash $Archive -Algorithm SHA256).Hash.ToLower()
    if ($actual -ne $expected) { Write-Error 'Checksum mismatch'; exit 1 }
  } catch {}
  Expand-Archive $Archive -DestinationPath $Dest
}
Remove-Item $Archive -Force -ErrorAction SilentlyContinue
$env:BINARYEN_HOME = $Dest
$env:PATH = "$($Dest)/bin;" + $env:PATH

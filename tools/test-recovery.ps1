# Safe v0.2.0 verification: never applies a real theme, removes a real component, or restarts Explorer.
[CmdletBinding()]
param([string]$EvidenceDir = (Join-Path $env:TEMP 'EasyRetro020-verification'), [switch]$SkipBuild)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$work = Join-Path $env:TEMP ('EasyRetro020-harness-' + [guid]::NewGuid().ToString('N'))
$root = 'Software\EasyRetroTest020-' + [guid]::NewGuid().ToString('N')
New-Item -ItemType Directory -Path $work, $EvidenceDir -Force | Out-Null
function Check-Exit([string]$Label) { if ($LASTEXITCODE -ne 0) { throw "$Label failed: exit $LASTEXITCODE" } }
try {
    Push-Location $repo
    try { if (!$SkipBuild) { & .\build.cmd; Check-Exit 'zero-warning /warnaserror+ build' } } finally { Pop-Location }
    $common = @('/nologo', '/platform:x64', '/langversion:5', '/codepage:65001', '/warn:4', '/warnaserror+', '/reference:System.dll', '/reference:System.Core.dll', '/reference:System.Drawing.dll', '/reference:System.Windows.Forms.dll', '/reference:System.Web.Extensions.dll', '/reference:System.IO.Compression.dll', '/reference:System.IO.Compression.FileSystem.dll')
    $harness = Join-Path $work 'RecoveryTests.exe'
    & $csc @common /target:exe /main:EasyRetro.RecoveryTests "/out:$harness" "/recurse:$repo\src\*.cs" (Join-Path $PSScriptRoot 'RecoveryTests.cs'); Check-Exit 'recovery harness compile'
    $gui = Join-Path $work 'GuiCheck.exe'
    & $csc @common /target:winexe "/out:$gui" (Join-Path $PSScriptRoot 'GuiCheck.cs'); Check-Exit 'GUI harness compile'
    $cli = Join-Path $work 'CliCheck.exe'
    & $csc @common /target:exe "/out:$cli" (Join-Path $PSScriptRoot 'CliCheck.cs'); Check-Exit 'CLI harness compile'
    $backup = Join-Path $work 'backup'
    & $harness capture $backup $root; Check-Exit 'synthetic capture'
    $script = Join-Path $PSScriptRoot 'easyretro-restore.ps1'
    $bytes = [IO.File]::ReadAllBytes($script)
    if ($bytes[0] -ne 239 -or $bytes[1] -ne 187 -or $bytes[2] -ne 191) { throw 'Restore text must be UTF-8 with BOM for PowerShell 5.1' }
    $readme = [IO.File]::ReadAllText((Join-Path $repo 'README.md'))
    $embedded = [regex]::Match($readme, '(?s)```powershell\r?\n(# EasyRetro 0\.2\.0.*?)\r?\n```')
    $source = [IO.File]::ReadAllText($script)
    if (!$embedded.Success -or ($embedded.Groups[1].Value.Replace("`r`n", "`n").TrimEnd() -cne $source.Replace("`r`n", "`n").TrimEnd())) { throw 'README embedded restore script must exactly match the downloadable text file' }
    Write-Host 'PASS UTF-8 BOM and README copy-paste script identity'
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $script -BackupDir $backup | Tee-Object -FilePath (Join-Path $EvidenceDir 'restore-dry-run.txt'); Check-Exit 'synthetic restore dry-run'
    & $harness mutated $backup $root; Check-Exit 'dry-run nonmutation assertion'
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $script -BackupDir $backup -Apply | Tee-Object -FilePath (Join-Path $EvidenceDir 'restore-apply.txt'); Check-Exit 'synthetic restore apply'
    & $harness restored $backup $root; Check-Exit 'exact round-trip assertion'
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $script -BackupDir $backup -Apply | Tee-Object -FilePath (Join-Path $EvidenceDir 'restore-idempotent.txt'); Check-Exit 'idempotent synthetic restore'
    & $harness damage-index $backup $root; Check-Exit 'partial backup preparation'
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $script -BackupDir $backup -Apply | Tee-Object -FilePath (Join-Path $EvidenceDir 'restore-partial.txt')
    if ($LASTEXITCODE -ne 1) { throw 'Missing JSON index must report an incomplete restore with exit 1' }
    & $harness salvaged $backup $root; Check-Exit 'remaining exports salvaged'
    & $harness export-restore $backup $root; Check-Exit 'app export-only fallback'
    & $harness uninstall $work $root; Check-Exit 'registered offline uninstaller/network sentinel'
    $exe = Join-Path $repo 'dist\EasyRetro.exe'
    $process = Start-Process -FilePath $cli -ArgumentList ('"' + $exe + '" "' + $EvidenceDir + '"') -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "CLI checks failed: see $EvidenceDir\cli-failure.txt" }
    Get-Content (Join-Path $EvidenceDir 'cli-summary.txt')
    $selftest = Get-Content (Join-Path $EvidenceDir 'selftest.txt') -Encoding UTF8
    $selftest | Where-Object { $_ -like 'PASS --selftest*' }
    & $harness assets (Join-Path $work 'assets') $root; Check-Exit 'visual asset checks'
    $guiLog = Join-Path $EvidenceDir 'gui-local.txt'
    $process = Start-Process -FilePath $gui -ArgumentList ('"' + $exe + '" "' + $guiLog + '"') -Wait -PassThru
    Get-Content $guiLog -Encoding UTF8
    if ($process.ExitCode -ne 0) { throw 'Consoleless GUI check failed' }
    Write-Host 'PASS all v0.2.0 safe local verification checks'
} finally {
    [Microsoft.Win32.Registry]::CurrentUser.DeleteSubKeyTree($root, $false)
    $key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($root)
    if ($null -ne $key) { $key.Dispose(); throw "Synthetic test key cleanup failed: HKCU\$root" }
    Write-Host "PASS synthetic key deleted: HKCU\$root"
    if (Test-Path -LiteralPath $work) { Remove-Item -LiteralPath $work -Recurse -Force }
    Write-Host "Evidence: $EvidenceDir (generated harness binaries/data removed)"
}

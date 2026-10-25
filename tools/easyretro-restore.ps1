# EasyRetro 0.2.0 - Windows PowerShell 5.1, offline, no external modules.
[CmdletBinding()]
param(
    [string]$BackupDir = (Join-Path $env:LOCALAPPDATA 'EasyRetro\backup'),
    [switch]$Apply,
    [switch]$RemoveComponents,
    [switch]$RestartShell,
    [string]$OriginalUserSid = ([Security.Principal.WindowsIdentity]::GetCurrent().User.Value)
)
$ErrorActionPreference = 'Stop'
$failures = New-Object 'System.Collections.Generic.List[string]'
$leftovers = New-Object 'System.Collections.Generic.List[string]'
if ($Apply -and $OriginalUserSid -ne [Security.Principal.WindowsIdentity]::GetCurrent().User.Value) {
    throw '다른 관리자 계정으로 복구할 수 없습니다. 원래 사용자 계정에서 실행하세요.'
}
function Restore-Item([string]$Label, [scriptblock]$Action) {
    if (!$Apply) { Write-Host "[계획/변경 없음] $Label"; return }
    try { & $Action; Write-Host "[복구] $Label" }
    catch { $message = "$Label : $($_.Exception.Message)"; $failures.Add($message); Write-Warning $message }
}
function Invoke-Native([string]$File, [string]$Arguments) {
    # reg.exe writes successful completion to stderr; PS 5.1 must not turn that into
    # a terminating NativeCommandError. Judge native programs by their exit codes.
    $info = New-Object Diagnostics.ProcessStartInfo
    $info.FileName = $File; $info.Arguments = $Arguments
    $info.UseShellExecute = $false; $info.CreateNoWindow = $true
    $info.RedirectStandardOutput = $true; $info.RedirectStandardError = $true
    $process = [Diagnostics.Process]::Start($info)
    try {
        $stdout = $process.StandardOutput.ReadToEndAsync(); $stderr = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $text = $stdout.Result + $stderr.Result
        if ($process.ExitCode -ne 0) { Write-Warning "$File : $text" }
        return $process.ExitCode
    } finally { $process.Dispose() }
}
function Read-Backup([string]$Name) {
    $file = Join-Path $BackupDir $Name
    if (!(Test-Path -LiteralPath $file)) { throw "원본 백업 파일 없음: $file" }
    Get-Content -LiteralPath $file -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Registry-Root([string]$Hive) {
    if (!$Hive -or $Hive -eq 'HKCU') { return [Microsoft.Win32.Registry]::CurrentUser }
    if ($Hive -eq 'HKLM') { return [Microsoft.Win32.Registry]::LocalMachine }
    throw "지원하지 않는 하이브: $Hive"
}
function Value-Data($Value) {
    switch ($Value.kind) {
        'Binary' { return ,([Convert]::FromBase64String($Value.data)) }
        'None' { return ,([Convert]::FromBase64String($Value.data)) }
        'MultiString' { return ,([string[]]@($Value.strings)) }
        'DWord' { return [int]::Parse($Value.data, [Globalization.CultureInfo]::InvariantCulture) }
        'QWord' { return [long]::Parse($Value.data, [Globalization.CultureInfo]::InvariantCulture) }
        'String' { return [string]$Value.data }
        'ExpandString' { return [string]$Value.data }
        default { throw "지원하지 않는 값 종류: $($Value.kind)" }
    }
}
function Value-Matches($Value) {
    $key = (Registry-Root $Value.hive).OpenSubKey($Value.key)
    try {
        $exists = $null -ne $key -and @($key.GetValueNames()) -contains [string]$Value.name
        if ($exists -ne [bool]$Value.existed) { return $false }
        if (!$exists) { return $true }
        if ($key.GetValueKind($Value.name).ToString() -ne $Value.kind) { return $false }
        $data = $key.GetValue($Value.name, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
        switch ($Value.kind) {
            { $_ -in 'Binary', 'None' } { return [Convert]::ToBase64String($data) -ceq $Value.data }
            'MultiString' {
                if (@($data).Count -ne @($Value.strings).Count) { return $false }
                for ($i = 0; $i -lt @($data).Count; $i++) { if ($data[$i] -cne $Value.strings[$i]) { return $false } }
                return $true
            }
            default { return [Convert]::ToString($data, [Globalization.CultureInfo]::InvariantCulture) -ceq $Value.data }
        }
    } finally { if ($null -ne $key) { $key.Dispose() } }
}
function Restore-Value($Value) {
    if (Value-Matches $Value) { return }
    $root = Registry-Root $Value.hive
    if ($Value.existed) {
        $key = $root.CreateSubKey($Value.key)
        try { $key.SetValue([string]$Value.name, (Value-Data $Value), [Microsoft.Win32.RegistryValueKind][Enum]::Parse([Microsoft.Win32.RegistryValueKind], $Value.kind)) }
        finally { $key.Dispose() }
    } else {
        $key = $root.OpenSubKey($Value.key, $true)
        try { if ($null -ne $key) { $key.DeleteValue([string]$Value.name, $false) } }
        finally { if ($null -ne $key) { $key.Dispose() } }
    }
}
function Key-Matches($Record) {
    $root = Registry-Root $Record.hive
    $key = $root.OpenSubKey($Record.key)
    try {
        if (!$Record.existed) { return $null -eq $key }
        if ($null -eq $key) { return $false }
    } finally { if ($null -ne $key) { $key.Dispose() } }
    if ($null -eq $Record.image) {
        $export = Join-Path $BackupDir $Record.file
        if (!(Test-Path -LiteralPath $export)) { throw "레지스트리 내보내기 백업 없음: $export" }
        $temporary = Join-Path ([IO.Path]::GetTempPath()) ('EasyRetro-reg-' + [guid]::NewGuid().ToString('N') + '.reg')
        try {
            $code = Invoke-Native 'reg.exe' ('export "' + $Record.hive + '\' + $Record.key + '" "' + $temporary + '" /y')
            if ($code -ne 0) { return $false }
            return [IO.File]::ReadAllText($temporary) -ceq [IO.File]::ReadAllText($export)
        } finally { if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary -Force } }
    }
    foreach ($image in $Record.image) {
        $key = $root.OpenSubKey($image.path)
        try {
            if ($null -eq $key -or $key.ValueCount -ne @($image.values).Count) { return $false }
            foreach ($value in $image.values) { if (!(Value-Matches $value)) { return $false } }
            if ($image.childrenCaptured) {
                $actual = @($key.GetSubKeyNames() | Sort-Object)
                $expected = @($image.subkeys | Sort-Object)
                if ($actual.Count -ne $expected.Count -or ($actual -join "`0") -ine ($expected -join "`0")) { return $false }
            }
        } finally { if ($null -ne $key) { $key.Dispose() } }
    }
    return $true
}
function Restore-Key($Record) {
    $root = Registry-Root $Record.hive
    if (!$Record.existed) { $root.DeleteSubKeyTree($Record.key, $false); return }
    $export = Join-Path $BackupDir $Record.file
    if (Test-Path -LiteralPath $export) {
        $code = Invoke-Native 'reg.exe' ('import "' + $export + '"')
        if ($code -ne 0) { throw "레지스트리 가져오기 실패: $export ($code)" }
    } elseif ($null -eq $Record.image) { throw "레지스트리 내보내기 백업 없음: $export" }
    if ($null -eq $Record.image) { return } # Legacy exports merge; never delete the live tree first.
    foreach ($image in @($Record.image)) {
        $key = $root.CreateSubKey($image.path); $key.Dispose()
        foreach ($value in @($image.values)) { Restore-Value $value }
    }
    $images = @($Record.image)
    for ($i = $images.Count - 1; $i -ge 0; $i--) {
        $image = $images[$i]; $key = $root.OpenSubKey($image.path, $true)
        try {
            foreach ($name in $key.GetValueNames()) { if (@($image.values | ForEach-Object { $_.name }) -notcontains $name) { $key.DeleteValue($name, $false) } }
            if ($image.childrenCaptured) { foreach ($name in $key.GetSubKeyNames()) { if (@($image.subkeys) -notcontains $name) { $key.DeleteSubKeyTree($name, $false) } } }
        } finally { $key.Dispose() }
    }
}
function Delete-Empty([string]$Hive, [string]$Path) {
    $root = Registry-Root $Hive; $key = $root.OpenSubKey($Path)
    if ($null -eq $key) { return }
    try { $empty = $key.ValueCount -eq 0 -and $key.SubKeyCount -eq 0 } finally { $key.Dispose() }
    if ($empty) { $root.DeleteSubKey($Path, $false) }
}
function Run-Registered([string]$Command, [string]$Defaults) {
    $command = [Environment]::ExpandEnvironmentVariables($Command).Trim()
    if ($command -match '^"([^"]+)"\s*(.*)$') { $file = $matches[1]; $arguments = $matches[2] }
    elseif ($command -match '^(.+?\.exe)\s*(.*)$') { $file = $matches[1]; $arguments = $matches[2] }
    else { throw "등록된 제거 명령을 해석할 수 없음: $command" }
    if (![IO.Path]::IsPathRooted($file)) { $file = Join-Path $env:SystemRoot "System32\$file" }
    if (!(Test-Path -LiteralPath $file -PathType Leaf)) { throw "등록된 제거 실행 파일 없음: $file" }
    if (!$arguments) { $arguments = $Defaults }
    $process = Start-Process -FilePath $file -ArgumentList $arguments -PassThru -Wait
    if ($process.ExitCode -notin 0, 3010, 1605, 1614) { throw "제거 종료 코드 $($process.ExitCode): $file" }
}
Write-Host "EasyRetro 0.2.0 텍스트 복구 / 백업: $BackupDir / 적용: $([bool]$Apply) (네트워크 사용 안 함)"
$snapshot = $null
try { $snapshot = Read-Backup 'values.json' } catch { $failures.Add($_.Exception.Message); Write-Warning $_.Exception.Message }
if ($null -eq $snapshot) {
    # Salvage exports even when the JSON index was lost; absent values/keys cannot be inferred.
    $snapshot = [pscustomobject]@{ values = @(); keys = @(); missingKeys = @() }
    if (Test-Path -LiteralPath $BackupDir) {
        foreach ($export in Get-ChildItem -LiteralPath $BackupDir -Filter 'key-*.reg' -File) {
            try {
                $header = [regex]::Match([IO.File]::ReadAllText($export.FullName), '(?m)^\[(HKEY_CURRENT_USER|HKEY_LOCAL_MACHINE)\\([^\r\n]+)\]\r?$')
                if (!$header.Success) { throw "백업 키 경로를 읽을 수 없음: $($export.FullName)" }
                $snapshot.keys += [pscustomobject]@{ module = 'RecoveredExport'; hive = $(if ($header.Groups[1].Value -eq 'HKEY_CURRENT_USER') { 'HKCU' } else { 'HKLM' }); key = $header.Groups[2].Value; existed = $true; image = $null; file = $export.Name }
            } catch { $failures.Add($_.Exception.Message); Write-Warning $_.Exception.Message }
        }
    }
}
# Elevate only a requested mutation requiring machine-wide access; HKCU-only backups need no admin.
$machine = @($snapshot.values) + @($snapshot.keys) | Where-Object { $_.hive -eq 'HKLM' }
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if ($Apply -and !$admin -and (@($machine).Count -gt 0 -or $RemoveComponents -or $RestartShell)) {
    Write-Host '요청한 시스템 복구에 관리자 권한이 필요합니다. 같은 사용자 계정의 UAC를 승인하세요.'
    if (!$PSCommandPath) { throw '붙여넣은 내용을 UTF-8 BOM .ps1 파일로 저장한 뒤 다시 실행하세요.' }
    $arguments = '-NoProfile -ExecutionPolicy Bypass -File "' + $PSCommandPath + '" -BackupDir "' + $BackupDir + '" -Apply -OriginalUserSid ' + $OriginalUserSid
    if ($RemoveComponents) { $arguments += ' -RemoveComponents' }; if ($RestartShell) { $arguments += ' -RestartShell' }
    $process = Start-Process powershell.exe -Verb RunAs -ArgumentList $arguments -Wait -PassThru
    exit $process.ExitCode
}
if ($Apply -and $null -eq ('EasyRetroRestoreNative' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class EasyRetroRestoreNative {
 [DllImport("user32.dll", CharSet=CharSet.Unicode, SetLastError=true)] public static extern bool SystemParametersInfo(uint action,uint param,IntPtr data,uint flags);
 [DllImport("user32.dll", EntryPoint="SystemParametersInfoW", CharSet=CharSet.Unicode, SetLastError=true)] public static extern bool Wallpaper(uint action,uint param,string data,uint flags);
 [DllImport("user32.dll")] public static extern bool SetSysColors(int count,int[] indexes,int[] colors);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern IntPtr SendMessageTimeout(IntPtr window,uint message,IntPtr w,string text,uint flags,uint timeout,out IntPtr result);
 public static void Spi(uint action,byte[] bytes) { IntPtr p=Marshal.AllocHGlobal(bytes.Length);try {Marshal.Copy(bytes,0,p,bytes.Length);if(!SystemParametersInfo(action,(uint)bytes.Length,p,3))throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());}finally{Marshal.FreeHGlobal(p);} }
}
'@
}
$modules = @(@($snapshot.values) + @($snapshot.keys) | ForEach-Object { $_.module } | Sort-Object -Unique)
if ($modules -contains 'SysColors' -or (Test-Path -LiteralPath (Join-Path $BackupDir 'syscolors.json'))) { Restore-Item '시스템 색상 (syscolors.json)' {
    [int[]]$colors = Read-Backup 'syscolors.json'
    [int[]]$indexes = @(0..24) + @(26..30)
    if ($colors.Length -ne $indexes.Length) { throw 'syscolors.json 색상 개수 불일치' }
    if (![EasyRetroRestoreNative]::SetSysColors($colors.Length, $indexes, $colors)) { throw 'SetSysColors 실패' }
} }
if ($modules -contains 'Fonts' -or (Test-Path -LiteralPath (Join-Path $BackupDir 'nonclientmetrics.bin')) -or (Test-Path -LiteralPath (Join-Path $BackupDir 'iconfont.bin'))) {
    foreach ($pair in @(@('nonclientmetrics.bin', 0x2a), @('iconfont.bin', 0x22))) {
        $name = $pair[0]; $action = $pair[1]
        Restore-Item "글꼴 $name" { [EasyRetroRestoreNative]::Spi($action, [IO.File]::ReadAllBytes((Join-Path $BackupDir $name))) }
    }
}
if ($modules -contains 'Wallpaper' -or (Test-Path -LiteralPath (Join-Path $BackupDir 'wallpaper-state.json'))) { Restore-Item '배경 SPI_SETDESKWALLPAPER (레지스트리 원본 재적용 전)' {
    $wallpaper = @($snapshot.values | Where-Object { $_.key -eq 'Control Panel\Desktop' -and $_.name -eq 'Wallpaper' })
    if ($wallpaper.Count -eq 0) { $wallpaper = @($snapshot.keys | ForEach-Object { $_.image } | ForEach-Object { $_.values } | Where-Object { $_.key -eq 'Control Panel\Desktop' -and $_.name -eq 'Wallpaper' }) }
    if ($wallpaper.Count -eq 0 -and (Test-Path -LiteralPath (Join-Path $BackupDir 'wallpaper-state.json'))) {
        $saved = Read-Backup 'wallpaper-state.json'
        if (@($saved.images).Count -gt 0) { $wallpaper = @([pscustomobject]@{ data = [string]$saved.images[0] }) }
    }
    if ($wallpaper.Count -eq 0) { throw '원본 Wallpaper 값 없음' }
    $desktopValues = @($snapshot.values) + @($snapshot.keys | ForEach-Object { $_.image } | ForEach-Object { $_.values })
    foreach ($value in $desktopValues) { if ($value.key -eq 'Control Panel\Desktop' -and $value.name -in 'WallpaperStyle', 'TileWallpaper') { Restore-Value $value } }
    if (![EasyRetroRestoreNative]::Wallpaper(0x14, 0, [string]$wallpaper[0].data, 3)) { throw 'SPI_SETDESKWALLPAPER 실패' }
} }
# Uninstallers can rewrite settings, so remove them before restoring the captured registry.
if ($RemoveComponents) { Restore-Item '등록된 ExplorerPatcher/Open-Shell 제거 프로그램만 실행 (다운로드 안 함)' {
    $seen = @{}
    foreach ($hive in @([Microsoft.Win32.RegistryHive]::CurrentUser, [Microsoft.Win32.RegistryHive]::LocalMachine)) {
        foreach ($view in @([Microsoft.Win32.RegistryView]::Registry64, [Microsoft.Win32.RegistryView]::Registry32)) {
            $root = [Microsoft.Win32.RegistryKey]::OpenBaseKey($hive, $view)
            try {
                $uninstall = $root.OpenSubKey('SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall')
                if ($null -eq $uninstall) { continue }
                try { foreach ($name in $uninstall.GetSubKeyNames()) {
                    $entry = $uninstall.OpenSubKey($name)
                    try {
                        if ($null -eq $entry) { continue }
                        $display = [string]$entry.GetValue('DisplayName')
                        if ($display -notmatch 'ExplorerPatcher|Open-Shell') { continue }
                        $registered = [string]$entry.GetValue('QuietUninstallString')
                        if (!$registered) { $registered = [string]$entry.GetValue('UninstallString') }
                        $identity = "$hive\$name\$registered"
                        if ($seen.ContainsKey($identity)) { continue }; $seen[$identity] = $true
                        $command = $registered; $code = [guid]::Empty
                        if ($display -match 'Open-Shell' -and [guid]::TryParse($name, [ref]$code)) { $command = 'msiexec.exe /x ' + $code.ToString('B') + ' /qn /norestart' }
                        try {
                            try { Run-Registered $command $(if ($display -match 'ExplorerPatcher') { '/uninstall_silent' } else { '/qn /norestart' }) }
                            catch { if ($command -ne $registered -and $registered) { Run-Registered $registered '/qn /norestart' } else { throw } }
                            $remaining = $uninstall.OpenSubKey($name)
                            if ($null -ne $remaining) { $remaining.Dispose(); throw "제거 후에도 앱 등록이 남음: $registered" }
                        } catch { $leftovers.Add("$display : 설정 → 앱에서 제거하거나 $registered 를 실행하세요. $($_.Exception.Message)"); Write-Warning $_.Exception.Message }
                    } finally { if ($null -ne $entry) { $entry.Dispose() } }
                } } finally { $uninstall.Dispose() }
            } finally { $root.Dispose() }
        }
    }
    $menu = Join-Path $env:ProgramFiles 'Open-Shell\StartMenu.exe'
    $location = [Microsoft.Win32.Registry]::LocalMachine.OpenSubKey('SOFTWARE\OpenShell\OpenShell')
    try { if ($null -ne $location -and $location.GetValue('Path')) { $menu = Join-Path ([string]$location.GetValue('Path')) 'StartMenu.exe' } }
    finally { if ($null -ne $location) { $location.Dispose() } }
    if (Test-Path -LiteralPath $menu) {
        try { Run-Registered ('"' + $menu + '" -exit') '' } catch { Write-Warning $_.Exception.Message }
        $leftovers.Add("Open-Shell : 설정 → 앱에서 제거하거나 $menu 를 확인하세요.")
    }
    $ep = Join-Path $env:WINDIR 'dxgi.dll'
    if (Test-Path -LiteralPath $ep) { $leftovers.Add("ExplorerPatcher : 설정 → 앱에서 제거하거나 $(Join-Path $env:ProgramFiles 'ExplorerPatcher\ep_setup.exe') 를 실행하세요.") }
} }
$keys = @($snapshot.keys)
for ($i = $keys.Count - 1; $i -ge 0; $i--) {
    $record = $keys[$i]; if ($null -eq $record) { continue }
    try { if (Key-Matches $record) { Write-Host "[일치/건너뜀] $($record.hive)\$($record.key)"; continue } }
    catch { $failures.Add($_.Exception.Message); Write-Warning $_.Exception.Message; continue }
    Restore-Item "키 $($record.hive)\$($record.key) / $($record.file)" { Restore-Key $record }
}
$values = @($snapshot.values)
for ($i = $values.Count - 1; $i -ge 0; $i--) {
    $value = $values[$i]; if ($null -eq $value) { continue }
    Restore-Item "값 $($value.hive)\$($value.key) [$($value.name)] 종류=$($value.kind) 존재=$($value.existed)" {
        Restore-Value $value
        if (!$value.keyExisted) { Delete-Empty $value.hive $value.key }
    }
}
foreach ($missing in @($snapshot.missingKeys | Sort-Object { $_.Length } -Descending)) {
    if (!$missing) { continue }; $hive = 'HKCU'; $path = $missing
    if ($missing -match '^(HKCU|HKLM)\\(.+)$') { $hive = $matches[1]; $path = $matches[2] }
    Restore-Item "원래 없던 빈 키 제거: $missing" { Delete-Empty $hive $path }
}
if ($modules -contains 'Cursors') { Restore-Item '원본 커서 구성표 다시 읽기 (SPI_SETCURSORS, fWinIni=0)' {
    if (![EasyRetroRestoreNative]::SystemParametersInfo(0x57, 0, [IntPtr]::Zero, 0)) { Write-Warning '커서 레지스트리는 복구되었습니다. 다시 로그인하면 반영됩니다.' }
} }
if ($modules | Where-Object { $_ -in 'Wallpaper', 'SysColors', 'Fonts', 'Sounds', 'Cursors', 'DwmAccent', 'DesktopIcons', 'Shell' }) {
    Restore-Item 'WM_SETTINGCHANGE 브로드캐스트 (소리/커서 구성표 기본값 포함)' {
        $result = [IntPtr]::Zero
        [void][EasyRetroRestoreNative]::SendMessageTimeout([IntPtr]0xffff, 0x1a, [IntPtr]::Zero, 'ImmersiveColorSet', 2, 2000, [ref]$result)
        [void][EasyRetroRestoreNative]::SendMessageTimeout([IntPtr]0xffff, 0x1a, [IntPtr]::Zero, 'Control Panel', 2, 2000, [ref]$result)
    }
}
if ($RestartShell) { Restore-Item 'Explorer 재시작: 현재 세션 종료 후 /RL LIMITED 작업으로 일반 권한 실행' {
    $task = 'EasyRetro-Restore-' + [guid]::NewGuid().ToString('N'); $user = [Security.Principal.WindowsIdentity]::GetCurrent().Name
    $code = Invoke-Native 'schtasks.exe' ('/Create /TN "' + $task + '" /SC ONCE /ST 00:00 /TR "' + $env:WINDIR + '\explorer.exe" /RU "' + $user + '" /RL LIMITED /IT /F')
    if ($code -ne 0) { throw '일반 권한 Explorer 예약 작업 생성 실패' }
    try {
        Get-Process explorer -ErrorAction SilentlyContinue | Where-Object { $_.SessionId -eq (Get-Process -Id $PID).SessionId } | Stop-Process -Force
        Start-Sleep -Seconds 3
        $code = Invoke-Native 'schtasks.exe' ('/Run /TN "' + $task + '"')
        if ($code -ne 0) { throw '일반 권한 Explorer 예약 작업 실행 실패' }
        Start-Sleep -Seconds 3
    } finally { [void](Invoke-Native 'schtasks.exe' ('/Delete /TN "' + $task + '" /F')) }
} }
if ($failures.Count -gt 0) {
    Write-Warning ("나머지 복구를 계속했습니다. 복구하지 못한 항목 (백업 보존):`n" + ($failures -join "`n"))
    exit 1
}
if ($leftovers.Count -gt 0) { Write-Warning ("EasyRetro 설정은 모두 복구했습니다. ExplorerPatcher/Open-Shell 프로그램만 남아 있습니다:`n" + ($leftovers -join "`n")) }
Write-Host $(if ($Apply) { '완료: 원본 설정 복구. 백업 파일은 보존했습니다.' } else { '계획 완료: 실제 시스템 변경 없음. 적용하려면 -Apply를 추가하세요.' })

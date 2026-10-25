using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
namespace EasyRetro {
 internal sealed class Dependency { public string Id,Version,Url,Sha256,InstallArgs,UninstallArgs; public string RegistryPath=@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"; }
 internal static class Dependencies {
  // Windows registry contracts used by the modules (HKCU unless explicitly marked HKLM):
  // Control Panel\Colors: all 30 ColorNames in ThemeSpec.cs (RGB strings), and Desktop Wallpaper,
  // WallpaperStyle, TileWallpaper; Cursors default name and Arrow/Help/AppStarting/Wait/Crosshair/
  // IBeam/NWPen/No/SizeNS/SizeWE/SizeNWSE/SizeNESW/SizeAll/UpArrow/Hand:
  // https://learn.microsoft.com/en-us/windows/win32/controls/themesfileformat-overview
  // HotTrackingColor/MenuHilight/MenuBar color indices:
  // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getsyscolor
  // Cursors Pin/Person and Scheme Source=0: Windows 11 in-box HKLM
  // SOFTWARE\Microsoft\Windows\CurrentVersion\Control Panel\Cursors\Schemes, verified with reg query;
  // scheme loading and reload contract:
  // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfow
  // Desktop\WindowMetrics CaptionFont/SmCaptionFont/MenuFont/StatusFont/MessageFont/IconFont are
  // changed indirectly by SPI_SETNONCLIENTMETRICS / SPI_SETICONTITLELOGFONT, never hand-encoded:
  // https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-nonclientmetricsw
  // AppEvents\Schemes default, Names\<scheme> default and Apps\<app>\<event>\.Current/<scheme>
  // defaults (WindowsLogon, WindowsLogoff, .Default, SystemHand, SystemNotification, EmptyRecycleBin):
  // https://learn.microsoft.com/en-us/windows/win32/controls/themesfileformat-overview#sounds-and-appevents-sections-sounds
  // DWM AccentColor/ColorizationColor/ColorPrevalence and Explorer\Accent AccentPalette/AccentColorMenu:
  // https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-rdperp/bc6975ee-c630-4414-ba10-04eecbb6fccc
  // Themes\Personalize ColorPrevalence, EnableTransparency, AppsUseLightTheme, SystemUsesLightTheme:
  // https://learn.microsoft.com/en-us/windows/apps/develop/settings/settings-common#personalization---colors
  // (that table misspells ColorPrevalence as ColorPrevalance; the actual Windows value is used).
  // Explorer\HideDesktopIcons\NewStartPanel Computer {20D04FE0-3AEA-1069-A2D8-08002B30309D},
  // Recycle Bin {645FF040-5081-101B-9F08-00AA002F954E}, DWORD 0=shown:
  // https://www.tweaknow.com/RegTweakHideDesktopIcons.php
  // SelfInstall Uninstall\EasyRetro DisplayName/DisplayVersion/Publisher/DisplayIcon/UninstallString/
  // NoModify/NoRepair follow the standard Add/Remove Programs registry contract:
  // https://learn.microsoft.com/en-us/windows/win32/msi/uninstall-registry-key
  // Release notes explicitly test 26100 and 26200: https://github.com/valinet/ExplorerPatcher/releases/tag/26100.8457.70.3
  // Commands: https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/ep_setup/ep_setup.c#L986-L998
  // /uninstall_silent skips the initial confirmation, but upstream can still show completion/restart/error dialogs (L1674-L1689).
  public static readonly Dependency EP=new Dependency {Id="explorerPatcher",Version="26100.8457.70.3",Url="https://github.com/valinet/ExplorerPatcher/releases/download/26100.8457.70.3/ep_setup.exe",Sha256="8146db4d3a87201fb80ad1d3712ba8f56883e9a0811758bd39f62d73a9f2c586",InstallArgs="/update_silent",UninstallArgs="/uninstall_silent"};
  // https://github.com/Open-Shell/Open-Shell-Menu/releases/tag/v4.4.198
  // Bootstrapper passes MSI args: Src/Setup/Setup.cpp; StartMenu feature: Src/Setup/Setup.wxs#L78
  public static readonly Dependency OS=new Dependency {Id="openShell",Version="4.4.198",Url="https://github.com/Open-Shell/Open-Shell-Menu/releases/download/v4.4.198/OpenShellSetup_4_4_198.exe",Sha256="a4d2d4459de55b5e962ba2a14f7bb794170511649138173dfa72949837b48c3f",InstallArgs="/qn ADDLOCAL=StartMenu NOSTART=1 /norestart",UninstallArgs="/x {ProductCode} /qn /norestart"};
  public const string EPKey=@"Software\ExplorerPatcher";
  public const string OSKey=@"Software\OpenShell\StartMenu\Settings";
  public const string Advanced=@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
  // ALL EP values: https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/ep_gui/resources/settings.reg#L8-L99
  // OldTaskbar=2 is the reimplemented Win10 taskbar required on 24H2+: https://github.com/valinet/ExplorerPatcher/wiki/ExplorerPatcher%27s-taskbar-implementation
  // OldTaskbarAl/MMOldTaskbarAl=0 left alignment; TaskbarGlomLevel/MMTaskbarGlomLevel=0 always combine; TaskbarSmallIcons=1/0.
  // Compact mode already uses the supported Advanced\TaskbarSmallIcons=1, not an invented EP value:
  // https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/ep_gui/resources/settings.reg#L97-L102
  public static List<Setting> Taskbar(ThemeSpec t) {return new List<Setting>{new Setting(EPKey,"OldTaskbar",NativeMethods.WindowsBuild()>=25921?2:1),new Setting(EPKey,"OldTaskbarAl",0),new Setting(EPKey,"MMOldTaskbarAl",0),new Setting(Advanced,"TaskbarGlomLevel",0),new Setting(Advanced,"MMTaskbarGlomLevel",0),new Setting(Advanced,"TaskbarSmallIcons",t.XP?0:1)};}
  // Every Open-Shell value below is defined here (pinned, not master):
  // https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/StartMenu/StartMenuDLL/SettingsUI.cpp#L4135-L4482
  // MenuStyle Classic1/Classic2 (L4137); WinKey ClassicMenu / ShiftWin WindowsMenu (L4179/L4187);
  // SearchBox Normal and SearchPrograms=1 (L4367/L4378); SkinC1/SkinC2 (L4435/L4440);
  // EnableStartButton, StartButtonType CustomButton, StartButtonPath, StartButtonSize (L4450-L4458);
  // CustomTaskbar (NOT SkinTaskbar), TaskbarLook Opaque, TaskbarTexture, TaskbarTextColor;
  // TaskbarTileH/V accept Tile (0) or Stretch (1), not arbitrary sizing modes:
  // https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/StartMenu/StartMenuDLL/SettingsUI.cpp#L4475-L4481
  // Radio settings use REG_SZ names; bool/color/int use DWORD, paths strings:
  // https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/Lib/Settings.cpp#L154-L205
  // Shipped names Classic Skin and Windows XP Luna: https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/Setup/Setup.wxs#L522-L540
  // ShowedStyle2 prevents first-use style UI: Src/Lib/Settings.cpp#L1962-L1966.
  public static List<Setting> Menu(ThemeSpec t,string assets) {return new List<Setting>{new Setting(@"Software\OpenShell\StartMenu","ShowedStyle2",1),new Setting(OSKey,"MenuStyle",t.XP?"Classic2":"Classic1"),new Setting(OSKey,"SkinC1","Classic Skin"),new Setting(OSKey,"SkinC2","Windows XP Luna"),new Setting(OSKey,"EnableStartButton",1),new Setting(OSKey,"StartButtonType","CustomButton"),new Setting(OSKey,"StartButtonPath",Path.Combine(assets,"start-"+t.Id+".png")),new Setting(OSKey,"StartButtonSize",0),new Setting(OSKey,"CustomTaskbar",1),new Setting(OSKey,"TaskbarLook","Opaque"),new Setting(OSKey,"TaskbarTexture",Path.Combine(assets,"taskbar-"+t.Id+".png")),new Setting(OSKey,"TaskbarTextColor",t.XP?0xffffff:0),new Setting(OSKey,"TaskbarTileH","Tile"),new Setting(OSKey,"TaskbarTileV","Stretch"),new Setting(OSKey,"SearchBox","Normal"),new Setting(OSKey,"SearchPrograms",1),new Setting(OSKey,"WinKey","ClassicMenu"),new Setting(OSKey,"ShiftWin","WindowsMenu")};}
  public static string OpenShellPath() {using(RegistryKey k=Registry.LocalMachine.OpenSubKey(@"SOFTWARE\OpenShell\OpenShell")){string p=k==null?null:k.GetValue("Path") as string;return Path.Combine(p??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Open-Shell"),"StartMenu.exe");}}
  static bool IsOpenShell(Dependency d){return d.Id==OS.Id;}
  public static ComponentState Detect(Dependency d) {
   foreach(RegistryHive hive in new[]{RegistryHive.CurrentUser,RegistryHive.LocalMachine})
   foreach(RegistryView view in new[]{RegistryView.Registry64,RegistryView.Registry32})
   using(RegistryKey h=RegistryKey.OpenBaseKey(hive,view))using(RegistryKey u=h.OpenSubKey(d.RegistryPath)) {
    if(u==null)continue;
    foreach(string n in u.GetSubKeyNames())using(RegistryKey k=u.OpenSubKey(n)) {
     if(k==null)continue;string name=Convert.ToString(k.GetValue("DisplayName"));
     if(name.IndexOf(IsOpenShell(d)?"Open-Shell":"ExplorerPatcher",StringComparison.OrdinalIgnoreCase)<0)continue;
     string command=Convert.ToString(k.GetValue("QuietUninstallString"));
     if(string.IsNullOrWhiteSpace(command))command=Convert.ToString(k.GetValue("UninstallString"));
     return new ComponentState{version=Convert.ToString(k.GetValue("DisplayVersion")),productCode=n,uninstallCommand=command};
    }
   }
   if(IsOpenShell(d)&&File.Exists(OpenShellPath()))return new ComponentState{version="existing"};
   return null;
  }
  public static void Backup(RegistryBackup backup) {
   // Pinned ep_setup.c writes policy keys, RunOnce cleanup, UndockingDisabled and
   // pnidui's ShellServiceObject outside Software\ExplorerPatcher.
   if(Detect(EP)==null) {
    backup.CaptureKey("InstallerEP",@"Software\Policies\Microsoft\Windows\Explorer");
    backup.CaptureKey("InstallerEP",@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer");
    backup.Capture("InstallerEP",@"Software\Microsoft\Windows\CurrentVersion\RunOnce","ExplorerPatcherCleanup");
    backup.Capture("InstallerEP","HKLM",@"SOFTWARE\Microsoft\Windows\CurrentVersion\Shell\Update\Packages","UndockingDisabled");
    backup.CaptureKey("InstallerEP","HKLM",@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\ShellServiceObjects\{C2796011-81BA-4148-8FCA-C6643245113F}");
    // ep_weather.c / ep_weather_host.idl: COM server and generated proxy registration.
    foreach(string path in new[]{@"CLSID\{A6EA9C2D-4982-4827-9204-0AC532959F6D}",@"AppID\{A6EA9C2D-4982-4827-9204-0AC532959F6D}",@"CLSID\{CDBF3734-F847-4F1B-B953-A605434DC1E7}",@"Interface\{CDBF3734-F847-4F1B-B953-A605434DC1E7}"})CaptureClasses(backup,"InstallerEP",path);
   }
   // Setup.wxs: AutoRun and TreatAs are not owned installation records.
   if(Detect(OS)==null) {
    backup.Capture("InstallerOS","HKLM",@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run","Open-Shell Start Menu");
    backup.CaptureKey("InstallerOS","HKLM",@"SOFTWARE\OpenShell");
    CaptureClasses(backup,"InstallerOS",@"CLSID\{ECD4FC4D-521C-11D0-B792-00A0C90312E1}\TreatAs");
    // StartMenuHelper/*.rgs and StartMenuHelper.cpp (including Windows 10 pin handlers).
    foreach(string id in new[]{"{E595F05F-903F-4318-8B0A-7F633B520D2B}","{D3214FBB-3CA1-406A-B3E8-3EB7C393A15E}","{82E749ED-B971-4550-BAF7-06AA2BF7E836}","{5AB14324-C087-42C1-B905-A0BFDB4E9532}"})CaptureClasses(backup,"InstallerOS",@"CLSID\"+id);
    CaptureClasses(backup,"InstallerOS",@"AppID\{62D2FBE4-89F7-48A5-A35F-DA2B8A3C54B7}");
    CaptureClasses(backup,"InstallerOS",@"AppID\StartMenuHelper.DLL");
    foreach(string type in new[]{"exefile","lnkfile","Folder","Launcher.ImmersiveApplication","Launcher.DesktopPackagedApplication","Launcher.SystemSettings"})CaptureClasses(backup,"InstallerOS",type+@"\ShellEx\ContextMenuHandlers\StartMenuExt");
   }
  }
  static void CaptureClasses(RegistryBackup backup,string module,string path) {
   foreach(string hive in new[]{"HKCU","HKLM"}) {
    backup.CaptureKey(module,hive,@"SOFTWARE\Classes\"+path);
    if(path.StartsWith(@"CLSID\",StringComparison.Ordinal))backup.CaptureKey(module,hive,@"SOFTWARE\Classes\Wow6432Node\"+path);
   }
  }
  public static void Prepare(AppState state,StateStore store) {
   foreach(Dependency d in new[]{EP,OS}) {
    if(state.components.ContainsKey(d.Id))continue;
    ComponentState c=Detect(d);
    if(c==null) {
     // Do not overwrite/uninstall an unregistered foreign or partial installation.
     string path=d==EP?Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"dxgi.dll"):OpenShellPath();
     if(File.Exists(path))throw new InvalidOperationException("등록되지 않은 기존 구성요소를 먼저 확인하세요: "+path);
     string directory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),d==EP?"ExplorerPatcher":"Open-Shell");
     if(Directory.Exists(directory)&&Directory.GetFileSystemEntries(directory).Length!=0)throw new InvalidOperationException("등록되지 않은 기존 구성요소 폴더를 먼저 확인하세요: "+directory);
     c=new ComponentState{installPlanned=true,version=d.Version,installer=Downloader.Fetch(d,store.PathFor("cache"))};
    }
    state.components[d.Id]=c;store.Save(state);
   }
  }
  public static void Install(Dependency d,AppState state,StateStore store) {
   ComponentState c=state.components[d.Id];
   if(Detect(d)!=null){if(d==OS&&!File.Exists(OpenShellPath()))throw new InvalidOperationException("기존 Open-Shell에 시작메뉴 기능이 없습니다. 기존 설치에서 StartMenu 기능을 설치한 뒤 다시 시도하세요.");if(c.installPlanned){c.installPlanned=false;store.Save(state);}return;}
   if(!c.installedByUs&&!c.installPlanned)throw new InvalidOperationException(d.Id+" 기존 설치를 찾을 수 없습니다. 해당 구성요소를 다시 설치하거나 원래대로 복구하세요.");
   c.installer=Downloader.Fetch(d,store.PathFor("cache"));
   if(Downloader.Hash(c.installer)!=d.Sha256)throw new InvalidDataException(d.Id+" 설치 직전 SHA-256 검증 실패");
   if(d==OS) {
    // Extract only, then journal the MSI identity BEFORE Windows Installer can mutate anything.
    // Setup.cpp's extract64 mode only writes OpenShellSetup64_4_4_198.msi in WorkingDirectory.
    string directory=Path.GetDirectoryName(c.installer);
    Processes.Run(c.installer,"extract64 /qn",directory);
    c.msi=Path.Combine(directory,"OpenShellSetup64_4_4_198.msi");
    c.productCode=NativeMethods.MsiProductCode(c.msi);
   }
   c.installedByUs=true;c.installPlanned=false;c.installPending=true;store.Save(state);
   Processes.Run(c.installer,d.InstallArgs,null,3010);
   ComponentState found=Detect(d);
   if(found==null||(d==OS&&!File.Exists(OpenShellPath()))||(d==EP&&!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"dxgi.dll"))))throw new InvalidOperationException(d.Id+" 설치 확인 실패");
   if(!string.IsNullOrEmpty(found.productCode))c.productCode=found.productCode;
   c.version=found.version;c.installPending=false;store.Save(state);
  }
  internal static string[] UninstallCommand(string command,string defaultArgs) {
   command=Environment.ExpandEnvironmentVariables(command??"").Trim();
   string file,args;
   if(command.StartsWith("\"",StringComparison.Ordinal)) {
    int end=command.IndexOf('"',1);if(end<0)throw new InvalidDataException("제거 명령의 닫는 따옴표가 없습니다: "+command);
    file=command.Substring(1,end-1);args=command.Substring(end+1).Trim();
   }else {
    int end=command.IndexOf(".exe",StringComparison.OrdinalIgnoreCase);
    if(end<0)throw new InvalidDataException("제거 실행 파일 경로를 확인할 수 없습니다: "+command);
    file=command.Substring(0,end+4);args=command.Substring(end+4).Trim();
   }
   if(!Path.IsPathRooted(file))file=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),file);
   if(!File.Exists(file))throw new FileNotFoundException("등록된 제거 실행 파일이 없습니다.",file);
   return new[]{file,args.Length==0?defaultArgs:args};
  }
  static void RegisteredRemove(Dependency d,string command) {
   string[] parsed=UninstallCommand(command,IsOpenShell(d)?"/qn /norestart":d.UninstallArgs);
   Log.Write("오프라인 등록 제거 프로그램: "+Processes.Q(parsed[0])+" "+parsed[1]);
   Processes.Run(parsed[0],parsed[1],null,3010,1605,1614);
  }
  static bool ComponentFilesRemain(Dependency d){return File.Exists(IsOpenShell(d)?OpenShellPath():Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"dxgi.dll"));}
  public static string Remove(Dependency d,AppState state,StateStore store) {
   ComponentState c;if(!state.components.TryGetValue(d.Id,out c)||!c.installedByUs)return null;
   ComponentState found=Detect(d);
   if(found==null&&!c.installPending&&!ComponentFilesRemain(d))return null;
   c.installPending=true;store.Save(state);
   List<Exception> failures=new List<Exception>();bool removed=false;
   string registered=found==null?c.uninstallCommand:found.uninstallCommand;
   if(found!=null&&!string.IsNullOrEmpty(found.uninstallCommand))c.uninstallCommand=found.uninstallCommand;
   if(IsOpenShell(d)) {
    if(string.IsNullOrEmpty(c.productCode)&&found!=null)c.productCode=found.productCode;
    Guid code;
    if(Guid.TryParse(c.productCode,out code))try {
     Processes.Run("msiexec.exe","/x "+code.ToString("B")+" /qn /norestart",null,3010,1605,1614);removed=true;
    }catch(Exception error){failures.Add(error);Log.Write(error.Message);}
   }
   if(!removed&&!string.IsNullOrWhiteSpace(registered))try {RegisteredRemove(d,registered);removed=true;}catch(Exception error){failures.Add(error);Log.Write(error.Message);}
   if(!removed&&!IsOpenShell(d)) {
    // Registry first; then hash-verified cache/offline directory; network is the last resort only.
    try {
     string cache=store.PathFor("cache");
     if(!string.IsNullOrEmpty(c.installer)&&File.Exists(c.installer)&&Downloader.Hash(c.installer)==d.Sha256) {
      Processes.Run(c.installer,d.UninstallArgs,null,3010);removed=true;
     }else {c.installer=Downloader.Fetch(d,cache);Processes.Run(c.installer,d.UninstallArgs,null,3010);removed=true;}
    }catch(Exception error){failures.Add(error);Log.Write(error.Message);}
   }
   if(IsOpenShell(d))try {ShellRestart.StopMenu();}catch(Exception error){failures.Add(error);Log.Write(error.Message);}
   if(Detect(d)==null&&!ComponentFilesRemain(d)) {c.installPending=false;store.Save(state);return null;}
   string path=registered;
   if(string.IsNullOrWhiteSpace(path))path=IsOpenShell(d)?OpenShellPath():Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),@"ExplorerPatcher\ep_setup.exe");
   string warning=(IsOpenShell(d)?"Open-Shell":"ExplorerPatcher")+": 설정 → 앱에서 제거하거나 "+path+" 를 실행하세요.";
   Log.Write("경고: "+warning+(failures.Count==0?"":" ("+failures[failures.Count-1].Message+")"));store.Save(state);return warning;
  }
 }
}

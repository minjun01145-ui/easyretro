using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
namespace EasyRetro {
 internal sealed class Dependency { public string Id,Version,Url,Sha256,InstallArgs,UninstallArgs; }
 internal static class Dependencies {
  // Windows registry contracts used by the other modules (all targets are HKCU):
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
  public static readonly Dependency OS=new Dependency {Id="openShell",Version="4.4.198",Url="https://github.com/Open-Shell/Open-Shell-Menu/releases/download/v4.4.198/OpenShellSetup_4_4_198.exe",Sha256="a4d2d4459de55b5e962ba2a14f7bb794170511649138173dfa72949837b48c3f",InstallArgs="/qn ADDLOCAL=StartMenu /norestart",UninstallArgs="/x {ProductCode} /qn /norestart"};
  public const string EPKey=@"Software\ExplorerPatcher";
  public const string OSKey=@"Software\OpenShell\StartMenu\Settings";
  public const string Advanced=@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
  // ALL EP values: https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/ep_gui/resources/settings.reg#L8-L99
  // OldTaskbar=2 is the reimplemented Win10 taskbar required on 24H2+: https://github.com/valinet/ExplorerPatcher/wiki/ExplorerPatcher%27s-taskbar-implementation
  // OldTaskbarAl/MMOldTaskbarAl=0 left alignment; TaskbarGlomLevel/MMTaskbarGlomLevel=0 always combine; TaskbarSmallIcons=1/0.
  public static List<Setting> Taskbar(ThemeSpec t) {return new List<Setting>{new Setting(EPKey,"OldTaskbar",NativeMethods.WindowsBuild()>=25921?2:1),new Setting(EPKey,"OldTaskbarAl",0),new Setting(EPKey,"MMOldTaskbarAl",0),new Setting(Advanced,"TaskbarGlomLevel",0),new Setting(Advanced,"MMTaskbarGlomLevel",0),new Setting(Advanced,"TaskbarSmallIcons",t.XP?0:1)};}
  // Every Open-Shell value below is defined here (pinned, not master):
  // https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/StartMenu/StartMenuDLL/SettingsUI.cpp#L4135-L4482
  // MenuStyle Classic1/Classic2 (L4137); WinKey ClassicMenu / ShiftWin WindowsMenu (L4179/L4187);
  // SearchBox Normal and SearchPrograms=1 (L4367/L4378); SkinC1/SkinC2 (L4435/L4440);
  // EnableStartButton, StartButtonType CustomButton, StartButtonPath, StartButtonSize (L4450-L4458);
  // CustomTaskbar (NOT SkinTaskbar), TaskbarLook Opaque, TaskbarTexture, TaskbarTextColor, TaskbarTileH/V Stretch (L4465-L4482).
  // Radio settings use REG_SZ names; bool/color/int use DWORD, paths strings:
  // https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/Lib/Settings.cpp#L154-L205
  // Shipped names Classic Skin and Windows XP Luna: https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/Setup/Setup.wxs#L522-L540
  public static List<Setting> Menu(ThemeSpec t,string assets) {return new List<Setting>{new Setting(OSKey,"MenuStyle",t.XP?"Classic2":"Classic1"),new Setting(OSKey,"SkinC1","Classic Skin"),new Setting(OSKey,"SkinC2","Windows XP Luna"),new Setting(OSKey,"EnableStartButton",1),new Setting(OSKey,"StartButtonType","CustomButton"),new Setting(OSKey,"StartButtonPath",Path.Combine(assets,"start-"+t.Id+".png")),new Setting(OSKey,"StartButtonSize",0),new Setting(OSKey,"CustomTaskbar",1),new Setting(OSKey,"TaskbarLook","Opaque"),new Setting(OSKey,"TaskbarTexture",Path.Combine(assets,"taskbar-"+t.Id+".png")),new Setting(OSKey,"TaskbarTextColor",t.XP?0xffffff:0),new Setting(OSKey,"TaskbarTileH","Stretch"),new Setting(OSKey,"TaskbarTileV","Stretch"),new Setting(OSKey,"SearchBox","Normal"),new Setting(OSKey,"SearchPrograms",1),new Setting(OSKey,"WinKey","ClassicMenu"),new Setting(OSKey,"ShiftWin","WindowsMenu")};}
  public static string OpenShellPath() {using(RegistryKey k=Registry.LocalMachine.OpenSubKey(@"SOFTWARE\OpenShell\OpenShell")){string p=k==null?null:k.GetValue("Path") as string;return Path.Combine(p??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Open-Shell"),"StartMenu.exe");}}
  public static ComponentState Detect(Dependency d) {
   foreach(RegistryView view in new[]{RegistryView.Registry64,RegistryView.Registry32})using(RegistryKey h=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,view))using(RegistryKey u=h.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall")) {if(u==null)continue;foreach(string n in u.GetSubKeyNames())using(RegistryKey k=u.OpenSubKey(n)) {string name=Convert.ToString(k.GetValue("DisplayName"));if((d==EP && name.IndexOf("ExplorerPatcher",StringComparison.OrdinalIgnoreCase)>=0)||(d==OS && name.IndexOf("Open-Shell",StringComparison.OrdinalIgnoreCase)>=0)) return new ComponentState{version=Convert.ToString(k.GetValue("DisplayVersion")),productCode=n};}}
   if(d==OS && File.Exists(OpenShellPath()))return new ComponentState{version="existing"};
   return null;
  }
  public static void Prepare(AppState state,StateStore store) {foreach(Dependency d in new[]{EP,OS}) {if(state.components.ContainsKey(d.Id))continue;ComponentState c=Detect(d);if(c==null)c=new ComponentState{installedByUs=true,version=d.Version,installer=Downloader.Fetch(d,store.PathFor("cache"))};state.components[d.Id]=c;store.Save(state);}}
  public static void Install(Dependency d,AppState state,StateStore store) {
   ComponentState c=state.components[d.Id];
   if(Detect(d)!=null)return;
   if(!c.installedByUs)throw new InvalidOperationException(d.Id+" 기존 설치를 찾을 수 없습니다. 해당 구성요소를 다시 설치하거나 원래대로 복구하세요.");
   if(!File.Exists(c.installer)||Downloader.Hash(c.installer)!=d.Sha256)throw new InvalidDataException(d.Id+" 설치 직전 SHA-256 검증 실패");
   // Persist before launch: EP can leave partial files even when no uninstall entry was written.
   c.installPending=true;
   store.Save(state);
   Processes.Run(c.installer,d.InstallArgs);
   ComponentState found=Detect(d);
   if(found==null)throw new InvalidOperationException(d.Id+" 설치 확인 실패");
   c.productCode=found.productCode;c.version=found.version;
   c.installPending=false;
   store.Save(state);
  }
  public static void Remove(Dependency d,AppState state) {
   ComponentState c;
   if(!state.components.TryGetValue(d.Id,out c)||!c.installedByUs)return;
   ComponentState found=Detect(d);
   if(found==null && (d==OS || !c.installPending))return;
   if(d==OS) {
    if(string.IsNullOrEmpty(c.productCode))c.productCode=found.productCode;
    Guid code;
    if(!Guid.TryParse(c.productCode,out code))throw new InvalidOperationException("Open-Shell MSI ProductCode를 찾을 수 없습니다.");
    Processes.Run("msiexec.exe","/x "+c.productCode+" /qn /norestart");
   }else {
    if(!File.Exists(c.installer)||Downloader.Hash(c.installer)!=d.Sha256)throw new InvalidDataException("EP 제거 파일 검증 실패");
    Processes.Run(c.installer,d.UninstallArgs);
   }
  }
 }
}

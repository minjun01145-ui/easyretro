using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.Win32;
namespace EasyRetro {
 internal sealed class ApplyContext { public StateStore Store;public AppState State;public string Assets {get{return Store.PathFor("assets");}} }
 internal interface IModule {string Id {get;}IEnumerable<string> Plan(ThemeSpec theme,ApplyContext ctx);void Backup(RegistryBackup backup,ApplyContext ctx);void Apply(ThemeSpec theme,ApplyContext ctx);void Restore(RegistryBackup backup,ApplyContext ctx);}
 internal abstract class RegistryModule:IModule {
  public abstract string Id {get;}public abstract List<Setting> Settings(ThemeSpec t,ApplyContext c);
  public virtual IEnumerable<string> Plan(ThemeSpec t,ApplyContext c) {foreach(Setting s in Settings(t,c))yield return s.Describe();}
  public virtual void Backup(RegistryBackup b,ApplyContext c) {foreach(Setting s in Settings(ThemeSpec.Get("98"),c))b.Capture(Id,s.Key,s.Name);foreach(Setting s in Settings(ThemeSpec.Get("xp"),c))b.Capture(Id,s.Key,s.Name);}
  public virtual void Apply(ThemeSpec t,ApplyContext c) {foreach(Setting s in Settings(t,c))RegistryBackup.Set(s.Key,s.Name,s.Value,s.Kind);}
  public virtual void Restore(RegistryBackup b,ApplyContext c) {b.Restore(Id);}
 }
 internal sealed class DwmAccentModule:RegistryModule {
  public override string Id {get{return "DwmAccent";}}
  public override List<Setting> Settings(ThemeSpec t,ApplyContext c) {
   string dwm=@"Software\Microsoft\Windows\DWM",p=@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",accent=@"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent";
   byte[] palette=new byte[32];
   // Eight COLORREF entries: RGB bytes followed by a zero reserved byte (not alpha).
   for(int i=0;i<8;i++){palette[i*4]=t.Accent.R;palette[i*4+1]=t.Accent.G;palette[i*4+2]=t.Accent.B;}
   return new List<Setting>{new Setting(dwm,"AccentColor",unchecked((int)0xff000000)|ThemeSpec.Bgr(t.Accent)),new Setting(dwm,"ColorizationColor",t.Accent.ToArgb()),new Setting(dwm,"ColorPrevalence",1),new Setting(p,"ColorPrevalence",1),new Setting(p,"EnableTransparency",0),new Setting(p,"AppsUseLightTheme",1),new Setting(p,"SystemUsesLightTheme",1),new Setting(accent,"AccentPalette",palette),new Setting(accent,"AccentColorMenu",unchecked((int)0xff000000)|ThemeSpec.Bgr(t.Accent))};
  }
  public override void Apply(ThemeSpec t,ApplyContext c){base.Apply(t,c);NativeMethods.Broadcast();}public override void Restore(RegistryBackup b,ApplyContext c){base.Restore(b,c);NativeMethods.Broadcast();}
 }
 internal sealed class DesktopIconsModule:RegistryModule {
  public override string Id {get{return "DesktopIcons";}}
  public override List<Setting> Settings(ThemeSpec t,ApplyContext c) {string k=@"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel";return new List<Setting>{new Setting(k,"{20D04FE0-3AEA-1069-A2D8-08002B30309D}",0),new Setting(k,"{645FF040-5081-101B-9F08-00AA002F954E}",0)};}
 }
 internal sealed class SoundsModule:RegistryModule {
  public override string Id {get{return "Sounds";}}
  static readonly string[] Events={"WindowsLogon","WindowsLogoff",".Default","SystemHand","SystemNotification","EmptyRecycleBin"};
  static readonly string[] Legacy={"tada","chimes","ding","chord","notify","recycle"};
  public override List<Setting> Settings(ThemeSpec t,ApplyContext c) {
   string root=@"AppEvents\Schemes",scheme=t.XP?"EasyRetroXP":"EasyRetro98";
   List<Setting> values=new List<Setting>{new Setting(root,"",scheme),new Setting(root+@"\Names\"+scheme,"",t.XP?"EasyRetro XP":"EasyRetro 98")};
   if(t.XP)foreach(XpSound sound in XpAssets.Sounds)foreach(string soundEvent in sound.Events)AddSound(values,root+@"\Apps\"+soundEvent,scheme,Path.Combine(c.Assets,sound.Name));
   else {
    foreach(XpSound sound in XpAssets.Sounds)foreach(string soundEvent in sound.Events)AddSound(values,root+@"\Apps\"+soundEvent,scheme,"");
    for(int i=0;i<Events.Length;i++){string app=Events[i]=="EmptyRecycleBin"?"Explorer":".Default";AddSound(values,root+@"\Apps\"+app+"\\"+Events[i],scheme,Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"Media",Legacy[i]+".wav"));}
   }
   return values;
  }
  static void AddSound(List<Setting> values,string key,string scheme,string file){values.Add(new Setting(key+@"\.Current","",file));values.Add(new Setting(key+"\\"+scheme,"",file));}
  public override void Apply(ThemeSpec t,ApplyContext c) {foreach(Setting s in Settings(t,c))if(s.Value is string && ((string)s.Value).EndsWith(".wav",StringComparison.OrdinalIgnoreCase) && !File.Exists((string)s.Value))throw new FileNotFoundException("효과음 파일 없음",(string)s.Value);base.Apply(t,c);}
 }
 internal sealed class CursorsModule:RegistryModule {
  public override string Id {get{return "Cursors";}}
  static readonly string[] Names={"Arrow","Help","AppStarting","Wait","Crosshair","IBeam","NWPen","No","SizeNS","SizeWE","SizeNWSE","SizeNESW","SizeAll","UpArrow","Hand","Pin","Person"};
  static string[] Select() {using(RegistryKey k=Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Control Panel\Cursors\Schemes")) {if(k==null)throw new InvalidOperationException("내장 커서 스킴 없음");foreach(string preferred in new[]{"Windows Standard (large)","Windows Standard (extra large)","Windows Black"}){string value=k.GetValue(preferred,null,RegistryValueOptions.DoNotExpandEnvironmentNames) as string;if(value==null)continue;string[] paths=value.Split(',');bool valid=paths.Length>=Names.Length;for(int i=0;valid&&i<Names.Length;i++)if(paths[i].Length>0&&!File.Exists(Environment.ExpandEnvironmentVariables(paths[i])))valid=false;if(valid)return paths;}throw new InvalidOperationException("기존 비-aero 내장 커서 파일을 찾을 수 없습니다.");}}
  public override List<Setting> Settings(ThemeSpec t,ApplyContext c) {string[] files=Select();List<Setting> values=new List<Setting>{new Setting(@"Control Panel\Cursors","",t.XP?"EasyRetro XP (Windows Standard)":"EasyRetro 98 (Windows Standard)"),new Setting(@"Control Panel\Cursors","Scheme Source",0)};for(int i=0;i<Names.Length;i++)values.Add(new Setting(@"Control Panel\Cursors",Names[i],Environment.ExpandEnvironmentVariables(files[i])));return values;}
  public override void Apply(ThemeSpec t,ApplyContext c) {base.Apply(t,c);NativeMethods.ReloadCursors();}
  public override void Restore(RegistryBackup b,ApplyContext c) {base.Restore(b,c);NativeMethods.ReloadCursors();}
 }
 internal sealed class ExplorerPatcherModule:RegistryModule {
  public override string Id {get{return "ExplorerPatcher";}}
  public override List<Setting> Settings(ThemeSpec t,ApplyContext c){return Dependencies.Taskbar(t);}
  public override IEnumerable<string> Plan(ThemeSpec t,ApplyContext c){yield return "기존 EP 재사용; 없으면 "+Dependencies.EP.Version+" 다운로드/SHA-256 검증 → "+Dependencies.EP.InstallArgs;foreach(string s in base.Plan(t,c))yield return s;yield return "탐색기 리본/명령바 옵션 변경 없음 (탭 유지)";}
  public override void Backup(RegistryBackup b,ApplyContext c){b.CaptureKey(Id,Dependencies.EPKey);b.CaptureKey(Id,Dependencies.Advanced);b.CaptureKey(Id,@"Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3");b.CaptureKey(Id,@"Software\Microsoft\Windows\CurrentVersion\Explorer\MMStuckRects3");b.Capture(Id,@"Software\Classes\CLSID\{2cc5ca98-6485-489a-920e-b3e88a6ccce3}\ShellFolder","Attributes");base.Backup(b,c);}
  public override void Apply(ThemeSpec t,ApplyContext c){Dependencies.Install(Dependencies.EP,c.State,c.Store);base.Apply(t,c);}
 }
 internal sealed class OpenShellModule:RegistryModule {
  public override string Id {get{return "OpenShell";}}
  public override List<Setting> Settings(ThemeSpec t,ApplyContext c){return Dependencies.Menu(t,c.Assets);}
  public override IEnumerable<string> Plan(ThemeSpec t,ApplyContext c){yield return "기존 Open-Shell 재사용; 없으면 "+Dependencies.OS.Version+" 다운로드/SHA-256 검증 → "+Dependencies.OS.InstallArgs;foreach(string s in base.Plan(t,c))yield return s;}
  public override void Backup(RegistryBackup b,ApplyContext c){b.CaptureKey(Id,@"Software\OpenShell");b.Capture(Id,@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartPage","OpenAtLogon");base.Backup(b,c);}
  public override void Apply(ThemeSpec t,ApplyContext c){Dependencies.Install(Dependencies.OS,c.State,c.Store);ShellRestart.StopMenu();base.Apply(t,c);}
  public override void Restore(RegistryBackup b,ApplyContext c){ShellRestart.StopMenu();base.Restore(b,c);}
 }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
namespace EasyRetro {
 internal sealed class WallpaperModule:RegistryModule {
  public override string Id {get{return "Wallpaper";}}
  public override List<Setting> Settings(ThemeSpec t,ApplyContext c){return new List<Setting>{new Setting(@"Control Panel\Desktop","Wallpaper",t.XP?Path.Combine(c.Assets,"wallpaper-xp.png"):""),new Setting(@"Control Panel\Desktop","WallpaperStyle",t.XP?"10":"0"),new Setting(@"Control Panel\Desktop","TileWallpaper","0"),new Setting(@"Control Panel\Colors","Background",t.XP?"70 142 219":"0 128 128")};}
  public override IEnumerable<string> Plan(ThemeSpec t,ApplyContext c){foreach(string s in base.Plan(t,c))yield return s;yield return "SPI_SETDESKWALLPAPER + 즉시 배경색; IDesktopWallpaper 모니터별 배경/슬라이드쇼 및 원본 캐시 캡처";}
  public override void Backup(RegistryBackup b,ApplyContext c){base.Backup(b,c);b.CaptureKey(Id,@"Control Panel\Desktop");b.CaptureKey(Id,@"Software\Microsoft\Windows\CurrentVersion\Themes");b.CaptureKey(Id,@"Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers");WallpaperState.Backup(b.Folder);}
  public override void Apply(ThemeSpec t,ApplyContext c){base.Apply(t,c);NativeMethods.Check(NativeMethods.SpiString(0x14,0,t.XP?Path.Combine(c.Assets,"wallpaper-xp.png"):"",3));NativeMethods.Check(NativeMethods.SetSysColors(1,new[]{1},new[]{ThemeSpec.Bgr(t.XP?Color.FromArgb(70,142,219):Color.Teal)}));}
  public override void Restore(RegistryBackup b,ApplyContext c){
   // APIs can normalize cached registry values; exact kind/data must be restored last.
   try{WallpaperState.Restore(b.Folder);}finally{base.Restore(b,c);}
  }
 }
 internal sealed class SysColorsModule:RegistryModule {
  public override string Id {get{return "SysColors";}}
  public override List<Setting> Settings(ThemeSpec t,ApplyContext c){List<Setting> values=new List<Setting>();Color[] colors=t.Colors();for(int i=0;i<colors.Length;i++)values.Add(new Setting(@"Control Panel\Colors",ThemeSpec.ColorNames[i],ThemeSpec.Rgb(colors[i])));return values;}
  public override IEnumerable<string> Plan(ThemeSpec t,ApplyContext c){foreach(string s in base.Plan(t,c))yield return s;yield return "GetSysColor 원본 캡처 / SetSysColors 즉시 적용";}
  public override void Backup(RegistryBackup b,ApplyContext c){base.Backup(b,c);int[] colors=new int[ThemeSpec.ColorIndexes.Length];for(int i=0;i<colors.Length;i++)colors[i]=NativeMethods.GetSysColor(ThemeSpec.ColorIndexes[i]);StateStore.WriteJson(Path.Combine(b.Folder,"syscolors.json"),colors);}
  public override void Apply(ThemeSpec t,ApplyContext c){base.Apply(t,c);Color[] colors=t.Colors();int[] raw=new int[colors.Length];for(int i=0;i<raw.Length;i++)raw[i]=ThemeSpec.Bgr(colors[i]);NativeMethods.Check(NativeMethods.SetSysColors(raw.Length,ThemeSpec.ColorIndexes,raw));}
  public override void Restore(RegistryBackup b,ApplyContext c){try{int[] colors=StateStore.ReadJson<int[]>(Path.Combine(b.Folder,"syscolors.json"));NativeMethods.Check(NativeMethods.SetSysColors(colors.Length,ThemeSpec.ColorIndexes,colors));}finally{base.Restore(b,c);}}
 }
 internal sealed class FontsModule:RegistryModule {
  public override string Id {get{return "Fonts";}}
  public override List<Setting> Settings(ThemeSpec t,ApplyContext c){return new List<Setting>();}
  public override IEnumerable<string> Plan(ThemeSpec t,ApplyContext c){yield return "SPI_GET/SETNONCLIENTMETRICS 全 원본 구조체 바이트 → "+t.Font;yield return "SPI_GET/SETICONTITLELOGFONT 원본 바이트 / SPIF_UPDATEINIFILE|SPIF_SENDCHANGE";yield return @"HKCU\Control Panel\Desktop\WindowMetrics: CaptionFont, SmCaptionFont, MenuFont, StatusFont, MessageFont, IconFont 값 단위 복구";}
  public override void Backup(RegistryBackup b,ApplyContext c){b.CaptureKey(Id,@"Control Panel\Desktop\WindowMetrics");foreach(string n in new[]{"CaptionFont","SmCaptionFont","MenuFont","StatusFont","MessageFont","IconFont"})b.Capture(Id,@"Control Panel\Desktop\WindowMetrics",n);Directory.CreateDirectory(b.Folder);File.WriteAllBytes(Path.Combine(b.Folder,"nonclientmetrics.bin"),NativeMethods.SpiGet(0x29,Marshal.SizeOf(typeof(NativeMethods.NONCLIENTMETRICS))));File.WriteAllBytes(Path.Combine(b.Folder,"iconfont.bin"),NativeMethods.SpiGet(0x1f,Marshal.SizeOf(typeof(NativeMethods.LOGFONT))));}
  public override void Apply(ThemeSpec t,ApplyContext c){NativeMethods.NONCLIENTMETRICS m=NativeMethods.Decode<NativeMethods.NONCLIENTMETRICS>(NativeMethods.SpiGet(0x29,Marshal.SizeOf(typeof(NativeMethods.NONCLIENTMETRICS))));m.caption.face=t.Font;m.smallCaption.face=t.Font;m.menu.face=t.Font;m.status.face=t.Font;m.message.face=t.Font;NativeMethods.SpiSet(0x2a,NativeMethods.Encode(m));NativeMethods.LOGFONT icon=NativeMethods.Decode<NativeMethods.LOGFONT>(NativeMethods.SpiGet(0x1f,Marshal.SizeOf(typeof(NativeMethods.LOGFONT))));icon.face=t.Font;NativeMethods.SpiSet(0x22,NativeMethods.Encode(icon));}
  public override void Restore(RegistryBackup b,ApplyContext c){
   List<Exception> errors=new List<Exception>();
   try{NativeMethods.SpiSet(0x2a,File.ReadAllBytes(Path.Combine(b.Folder,"nonclientmetrics.bin")));}catch(Exception error){errors.Add(error);}
   try{NativeMethods.SpiSet(0x22,File.ReadAllBytes(Path.Combine(b.Folder,"iconfont.bin")));}catch(Exception error){errors.Add(error);}
   try{base.Restore(b,c);}catch(Exception error){errors.Add(error);}
   if(errors.Count>0)throw new AggregateException("글꼴 원본 복구 실패. 백업을 보존했습니다.",errors);
  }
 }
 internal sealed class ShellModule:RegistryModule {
  public override string Id {get{return "Shell";}}
  public override List<Setting> Settings(ThemeSpec t,ApplyContext c){return new List<Setting>();}
  public override IEnumerable<string> Plan(ThemeSpec t,ApplyContext c){yield return "설치/설정 기록 후 마지막 WM_SETTINGCHANGE; 현재 세션 모든 explorer.exe 종료 (EP가 띄운 상승 Explorer 포함); 자동 재시작 8초 대기";yield return "자동 재시작 실패 시 원 사용자 /RL LIMITED /IT 작업 생성→실행→삭제; 일반 권한 토큰 확인; Open-Shell도 LIMITED 시작";}
  public override void Backup(RegistryBackup b,ApplyContext c){File.WriteAllText(Path.Combine(b.Folder,"shell-user.txt"),System.Security.Principal.WindowsIdentity.GetCurrent().User.Value);}
  public override void Apply(ThemeSpec t,ApplyContext c){ShellRestart.Restart();}
  public override void Restore(RegistryBackup b,ApplyContext c){NativeMethods.Broadcast();}
 }
}

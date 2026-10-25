using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Win32;
namespace EasyRetro {
 internal static class Verification {
  static void Print(string text){ConsoleOutput.WriteLine(text);}
  static void Read(string label,Func<string> action){try{Print(label+": "+action());}catch(Exception error){Print(label+": 확인 불가 ("+error.Message+")");}}
  static RegistryKey Root(string hive){return hive=="HKLM"?Registry.LocalMachine:Registry.CurrentUser;}
  static string Value(string key,string name) {
   using(RegistryKey k=Registry.CurrentUser.OpenSubKey(key)) {
    object value=k==null?null:k.GetValue(name,null,RegistryValueOptions.DoNotExpandEnvironmentNames);
    if(value==null)return "(없음)";
    byte[] bytes=value as byte[];if(bytes!=null)return BitConverter.ToString(bytes);
    string[] strings=value as string[];if(strings!=null)return string.Join(" | ",strings);
    if(value is int)return "0x"+((int)value).ToString("X8",CultureInfo.InvariantCulture);
    return Convert.ToString(value,CultureInfo.InvariantCulture);
   }
  }
  static string Difference(RegistrySnapshot snapshot,string module) {
   if(snapshot==null)return "원본 스냅샷 없음 (비교 불가)";
   int differing=0,unreadable=0;HashSet<string> seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);bool captured=false;
   Action<ValueRecord> compare=delegate(ValueRecord value) {
    if(!seen.Add((value.hive??"HKCU")+"\\"+value.key+"\\"+value.name))return;
    try{if(!RegistryBackup.Matches(value))differing++;}catch(Exception){unreadable++;}
   };
   foreach(ValueRecord value in snapshot.values)if(value.module==module){captured=true;compare(value);}
   foreach(KeyRecord record in snapshot.keys)if(record.module==module) {
    captured=true;
    try {
     using(RegistryKey key=Root(record.hive).OpenSubKey(record.key)) {
      if(!record.existed){if(key!=null)differing++;continue;}
      if(key==null){differing++;continue;}
     }
     if(record.image==null){unreadable++;continue;}
     foreach(KeyImage image in record.image) {
      foreach(ValueRecord value in image.values)compare(value);
      using(RegistryKey key=Root(record.hive).OpenSubKey(image.path)) {
       if(key==null){differing++;continue;}
       foreach(string name in key.GetValueNames())if(!image.values.Exists(delegate(ValueRecord value){return string.Equals(value.name,name,StringComparison.OrdinalIgnoreCase);}))differing++;
       if(image.childrenCaptured)foreach(string name in key.GetSubKeyNames())if(!image.subkeys.Exists(delegate(string child){return string.Equals(child,name,StringComparison.OrdinalIgnoreCase);}))differing++;
      }
     }
    }catch(Exception){unreadable++;}
   }
   return captured?differing+"개"+(unreadable==0?"":" / 확인 불가 "+unreadable+"개 (권한 또는 구버전 .reg 백업)"):"캡처된 레지스트리 항목 없음";
  }
  static string Exists(string path){return (File.Exists(path)||Directory.Exists(path)?"있음: ":"없음: ")+path;}
  public static void Run(StateStore store) {
   Print("EasyRetro 0.2.0 읽기 전용 검증 (현재 사용자 / 설정 변경 없음)");
   Read("적용된 테마",delegate{AppState state=store.Load();return state==null?"없음":(state.appliedTheme??"없음")+" / "+state.operation;});
   RegistrySnapshot snapshot=null;
   string file=store.PathFor("backup/values.json");
   if(File.Exists(file))try{snapshot=StateStore.ReadJson<RegistrySnapshot>(file);}catch(Exception error){Print("원본 스냅샷 읽기 실패: "+error.Message);}
   Print("원본 스냅샷: "+file);Print("모듈 | 남아 있는 변경 (원본 대비)");
   foreach(string module in new[]{"Wallpaper","SysColors","DwmAccent","Fonts","Sounds","Cursors","DesktopIcons","ExplorerPatcher","OpenShell","InstallerEP","InstallerOS","ManagerRegistration"}) {
    string id=module;Read(id=="SysColors"?"Colors":id,delegate{return Difference(snapshot,id);});
   }
   Read("현재 Wallpaper 경로",delegate{return Value(@"Control Panel\Desktop","Wallpaper");});
   Read("현재 소리 구성표",delegate{return Value(@"AppEvents\Schemes","");});
   Read("현재 커서 구성표",delegate{return Value(@"Control Panel\Cursors","");});
   Read("현재 DWM 강조색",delegate{return Value(@"Software\Microsoft\Windows\DWM","AccentColor");});
   Read("현재 DesktopIcons 컴퓨터/휴지통",delegate{return Value(@"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel","{20D04FE0-3AEA-1069-A2D8-08002B30309D}")+" / "+Value(@"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel","{645FF040-5081-101B-9F08-00AA002F954E}");});
   foreach(Dependency dependency in new[]{Dependencies.EP,Dependencies.OS}) {
    Dependency item=dependency;Read(item.Id+" 설치 등록",delegate{ComponentState component=Dependencies.Detect(item);return component==null?"없음":"있음 / "+component.version;});
   }
   Read("현재 EP 작업표시줄 설정",delegate{return "OldTaskbar="+Value(Dependencies.EPKey,"OldTaskbar")+", TaskbarGlomLevel="+Value(Dependencies.Advanced,"TaskbarGlomLevel");});
   Read("현재 Open-Shell 설정",delegate{return "MenuStyle="+Value(Dependencies.OSKey,"MenuStyle")+", SkinC1="+Value(Dependencies.OSKey,"SkinC1")+", SkinC2="+Value(Dependencies.OSKey,"SkinC2");});
   Read("EP 쉘 파일",delegate{return Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"dxgi.dll"));});
   Read("EP 프로그램 폴더",delegate{return Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"ExplorerPatcher"));});
   Read("Open-Shell 시작메뉴 파일",delegate{return Exists(Dependencies.OpenShellPath());});
   Read("매니저 파일",delegate{return Exists(Path.Combine(SelfInstall.Folder,"EasyRetro.exe"));});
   Read("매니저 바로가기",delegate{return Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),"EasyRetro 매니저.lnk"));});
   Print("검증 완료: 차이/접근 불가 여부와 관계없이 종료 코드 0. 복구 작업은 실행하지 않았습니다.");
  }
 }
}

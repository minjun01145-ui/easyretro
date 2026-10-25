using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
namespace EasyRetro {
 internal static class SelfInstall {
  public static string Folder {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Programs","EasyRetro");}}
  static string Shortcut {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),"EasyRetro 매니저.lnk");}}
  public const string UninstallKey=@"Software\Microsoft\Windows\CurrentVersion\Uninstall\EasyRetro";
  public static void Backup(RegistryBackup backup) {
   backup.CaptureKey("ManagerRegistration",UninstallKey);
   string exe=Path.Combine(Folder,"EasyRetro.exe");
   StateStore.WriteJson(Path.Combine(backup.Folder,"manager-files.json"),new[]{File.Exists(exe),File.Exists(Shortcut)});
   if(File.Exists(exe))File.Copy(exe,Path.Combine(backup.Folder,"manager.exe"),true);
   if(File.Exists(Shortcut))File.Copy(Shortcut,Path.Combine(backup.Folder,"manager.lnk"),true);
  }
  public static void Rollback(RegistryBackup backup) {
   bool[] existed=StateStore.ReadJson<bool[]>(Path.Combine(backup.Folder,"manager-files.json"));
   string exe=Path.Combine(Folder,"EasyRetro.exe");
   if(existed[0]){string source=Path.Combine(backup.Folder,"manager.exe");if(!File.Exists(exe)||Downloader.Hash(source)!=Downloader.Hash(exe))File.Copy(source,exe,true);}
   else if(File.Exists(exe))try{File.Delete(exe);}catch(IOException error){if((error.HResult&0xffff)!=32&&(error.HResult&0xffff)!=33)throw;ScheduleRemoval(false);}
   if(existed[1])File.Copy(Path.Combine(backup.Folder,"manager.lnk"),Shortcut,true);else if(File.Exists(Shortcut))File.Delete(Shortcut);
   backup.Restore("ManagerRegistration");
   if(Directory.Exists(Folder)&&Directory.GetFileSystemEntries(Folder).Length==0)Directory.Delete(Folder);
  }
  public static void Install() {
   Directory.CreateDirectory(Folder);string exe=Path.Combine(Folder,"EasyRetro.exe");if(!string.Equals(Application.ExecutablePath,exe,StringComparison.OrdinalIgnoreCase)&&(!File.Exists(exe)||Downloader.Hash(exe)!=Downloader.Hash(Application.ExecutablePath)))File.Copy(Application.ExecutablePath,exe,true);
   Type type=Type.GetTypeFromProgID("WScript.Shell");object shell=Activator.CreateInstance(type);object link=null;try {link=type.InvokeMember("CreateShortcut",System.Reflection.BindingFlags.InvokeMethod,null,shell,new object[]{Shortcut});Type lt=link.GetType();lt.InvokeMember("TargetPath",System.Reflection.BindingFlags.SetProperty,null,link,new object[]{exe});lt.InvokeMember("WorkingDirectory",System.Reflection.BindingFlags.SetProperty,null,link,new object[]{Folder});lt.InvokeMember("Description",System.Reflection.BindingFlags.SetProperty,null,link,new object[]{"EasyRetro 테마 변경 및 원상복구"});lt.InvokeMember("Save",System.Reflection.BindingFlags.InvokeMethod,null,link,null);}finally {if(link!=null)Marshal.FinalReleaseComObject(link);Marshal.FinalReleaseComObject(shell);}
   using(RegistryKey k=Registry.CurrentUser.CreateSubKey(UninstallKey)){k.SetValue("DisplayName","EasyRetro");k.SetValue("UninstallString",Processes.Q(exe)+" --uninstall");k.SetValue("DisplayIcon",exe);k.SetValue("Publisher","EasyRetro contributors");k.SetValue("DisplayVersion","0.1.0");k.SetValue("NoModify",1,RegistryValueKind.DWord);k.SetValue("NoRepair",1,RegistryValueKind.DWord);}
  }
  public static void Remove() {
   if(File.Exists(Shortcut))File.Delete(Shortcut);
   Registry.CurrentUser.DeleteSubKeyTree(UninstallKey,false);
   ScheduleRemoval(true);
  }
  static void ScheduleRemoval(bool wholeFolder) {
   if(!Directory.Exists(Folder))return;
   string script=Path.Combine(Path.GetTempPath(),"easyretro-remove-"+Guid.NewGuid().ToString("N")+".ps1");
   string escaped=Folder.Replace("'","''");
   // The completion dialog can remain open indefinitely. Wait for every installed-manager process,
   // rather than racing a fixed delay against a locked executable; never terminate those processes.
   string content="$ErrorActionPreference='Stop'\n$folder='"+escaped+"'\n"+
    "do { Start-Sleep -Seconds 1; $running=@(Get-Process EasyRetro -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq ($folder+'\\EasyRetro.exe') }) } while($running.Count -gt 0)\n"+
    (wholeFolder?"try { Remove-Item -LiteralPath $folder -Recurse -Force }":"try { Remove-Item -LiteralPath ($folder+'\\EasyRetro.exe') -Force; if (@(Get-ChildItem -LiteralPath $folder -Force).Count -eq 0) { Remove-Item -LiteralPath $folder -Force } }")+" finally { Remove-Item -LiteralPath $PSCommandPath -Force }\n";
   File.WriteAllText(script,content,System.Text.Encoding.UTF8);
   ProcessStartInfo p=new ProcessStartInfo("powershell.exe","-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "+Processes.Q(script));
   p.UseShellExecute=false;p.CreateNoWindow=true;
   Process.Start(p).Dispose();
  }
 }
}

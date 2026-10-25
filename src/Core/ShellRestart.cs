using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
namespace EasyRetro {
 internal static class ShellRestart {
  static uint ShellPid(){uint pid=0;IntPtr h=NativeMethods.FindWindow("Shell_TrayWnd",null);if(h!=IntPtr.Zero)NativeMethods.GetWindowThreadProcessId(h,out pid);return pid;}
  public static void StopMenu(){string path=Dependencies.OpenShellPath();if(System.IO.File.Exists(path))Processes.Run(path,"-exit");}
  static void StopExplorers() {
   int session=Process.GetCurrentProcess().SessionId;
   foreach(Process process in Process.GetProcessesByName("explorer"))using(process) {
    try{if(process.SessionId!=session)continue;process.Kill();if(!process.WaitForExit(5000))throw new InvalidOperationException("Explorer 종료 대기 시간 초과");}
    catch(ArgumentException){/* The process exited between enumeration and termination. */}
    catch(InvalidOperationException){if(!process.HasExited)throw;}
   }
  }
  static bool Ready() {
   uint pid=ShellPid();if(pid==0)return false;
   try{using(Process process=Process.GetProcessById((int)pid)){if(process.SessionId!=Process.GetCurrentProcess().SessionId)return false;if(NativeMethods.ProcessElevated(process.Handle)){process.Kill();process.WaitForExit(5000);return false;}return true;}}
   catch(ArgumentException){return false;}
  }
  public static void Restart() {
   NativeMethods.Broadcast();StopMenu();IntPtr window=NativeMethods.FindWindow("Shell_TrayWnd",null);
   if(window!=IntPtr.Zero){NativeMethods.PostMessage(window,0x5b4,IntPtr.Zero,IntPtr.Zero);Thread.Sleep(1200);}
   // EP's upstream installer can itself launch Explorer. Reap ALL Explorer processes
   // in this interactive session, not just the old tray PID (which may already have changed).
   StopExplorers();DateTime deadline=DateTime.UtcNow.AddSeconds(8);
   while(DateTime.UtcNow<deadline){if(Ready()){StartMenuLimited();return;}Thread.Sleep(250);}
   RunLimited(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"explorer.exe"),"");
   deadline=DateTime.UtcNow.AddSeconds(8);
   while(DateTime.UtcNow<deadline){if(Ready()){StartMenuLimited();return;}Thread.Sleep(250);}
   throw new InvalidOperationException("일반 권한 Explorer 재시작 실패. 작업 관리자에서 explorer.exe를 실행하세요.");
  }
  static void StartMenuLimited(){string path=Dependencies.OpenShellPath();if(System.IO.File.Exists(path))RunLimited(path,"-startup");}
  public static void RunLimited(string file,string args) {
   // /RL LIMITED and interactive logon are mandatory. Never directly launch a shell elevated.
   string name="EasyRetro-"+Guid.NewGuid().ToString("N");string user=WindowsIdentity.GetCurrent().Name;string action=Processes.Q(file)+(args.Length==0?"":" "+args);bool created=false;
   try{Processes.Run("schtasks.exe","/Create /TN "+name+" /TR "+Processes.Q(action)+" /SC ONCE /ST 23:59 /RU "+Processes.Q(user)+" /RL LIMITED /IT /F");created=true;Processes.Run("schtasks.exe","/Run /TN "+name);Thread.Sleep(1200);}
   finally{if(created)Processes.Run("schtasks.exe","/Delete /TN "+name+" /F");}
  }
 }
}

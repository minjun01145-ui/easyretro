using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
namespace EasyRetro {
 internal static class ShellRestart {
  static uint ShellPid() {uint pid=0;IntPtr h=NativeMethods.FindWindow("Shell_TrayWnd",null);if(h!=IntPtr.Zero)NativeMethods.GetWindowThreadProcessId(h,out pid);return pid;}
  public static void Restart() {
   NativeMethods.Broadcast();uint old=ShellPid();if(old!=0){IntPtr h=NativeMethods.FindWindow("Shell_TrayWnd",null);NativeMethods.PostMessage(h,0x5b4,IntPtr.Zero,IntPtr.Zero);Thread.Sleep(1600);if(ShellPid()==old){using(Process p=Process.GetProcessById((int)old)){if(p.SessionId!=Process.GetCurrentProcess().SessionId)throw new InvalidOperationException("다른 세션의 Explorer는 종료하지 않습니다.");p.Kill();p.WaitForExit(4000);}}}
   DateTime deadline=DateTime.UtcNow.AddSeconds(8);while(DateTime.UtcNow<deadline){uint pid=ShellPid();if(pid!=0&&pid!=old){StartMenuLimited();return;}Thread.Sleep(250);}
   RunLimited("explorer.exe","");deadline=DateTime.UtcNow.AddSeconds(8);while(ShellPid()==0&&DateTime.UtcNow<deadline)Thread.Sleep(250);if(ShellPid()==0)throw new InvalidOperationException("일반 권한 Explorer 재시작 실패. 작업 관리자에서 explorer.exe를 실행하세요.");StartMenuLimited();
  }
  static void StartMenuLimited() {string path=Dependencies.OpenShellPath();if(System.IO.File.Exists(path))RunLimited(path,"-startup");}
  public static void RunLimited(string file,string args) {
   // Task scheduler interactive-token task. /RL LIMITED is essential: never spawn Explorer with the elevated token.
   string name="EasyRetro-"+Guid.NewGuid().ToString("N");string user=WindowsIdentity.GetCurrent().Name;string action=Processes.Q(file)+(args.Length==0?"":" "+args);
   try {Processes.Run("schtasks.exe","/Create /TN "+name+" /TR "+Processes.Q(action)+" /SC ONCE /ST 23:59 /RU "+Processes.Q(user)+" /RL LIMITED /IT /F");Processes.Run("schtasks.exe","/Run /TN "+name);Thread.Sleep(1200);}finally {Processes.Run("schtasks.exe","/Delete /TN "+name+" /F");}
  }
 }
}

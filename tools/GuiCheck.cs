using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
internal static class GuiCheck {
 [DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool ReadProcessMemory(IntPtr process,IntPtr address,byte[] bytes,int size,out IntPtr read);
 [DllImport("ntdll.dll")] static extern int NtQueryInformationProcess(IntPtr process,int kind,IntPtr[] data,int size,out int returned);
 [DllImport("user32.dll",SetLastError=true)] static extern IntPtr SendMessageTimeout(IntPtr window,uint message,IntPtr w,IntPtr l,uint flags,uint timeout,out IntPtr result);
 [DllImport("user32.dll")] static extern bool PostMessage(IntPtr window,uint message,IntPtr w,IntPtr l);
 static IntPtr Pointer(Process child,IntPtr address) {byte[] bytes=new byte[8];IntPtr count;if(!ReadProcessMemory(child.Handle,address,bytes,8,out count)||count.ToInt64()!=8)throw new InvalidOperationException("ReadProcessMemory failed");return new IntPtr(BitConverter.ToInt64(bytes,0));}
 static void Require(bool condition,string text){if(!condition)throw new InvalidOperationException(text);}
 [STAThread] static int Main(string[] args) {
  StringBuilder evidence=new StringBuilder();Process child=null;
  try {
   Require(GetConsoleWindow()==IntPtr.Zero,"GUI parent has a console");evidence.AppendLine("PASS winexe parent GetConsoleWindow=0");
   child=Process.Start(new ProcessStartInfo(args[0]){UseShellExecute=false,CreateNoWindow=true});
   for(int i=0;i<100;i++){Thread.Sleep(100);child.Refresh();if(child.HasExited)throw new InvalidOperationException("GUI exited early: "+child.ExitCode);if(child.MainWindowHandle!=IntPtr.Zero&&child.MainWindowTitle=="EasyRetro")break;}
   Require(child.MainWindowHandle!=IntPtr.Zero&&child.MainWindowTitle=="EasyRetro","Manager main window missing or error dialog shown");
   IntPtr result;Require(SendMessageTimeout(child.MainWindowHandle,0,IntPtr.Zero,IntPtr.Zero,2,2000,out result)!=IntPtr.Zero,"Manager unresponsive");
   IntPtr[] info=new IntPtr[6];int returned;Require(NtQueryInformationProcess(child.Handle,0,info,48,out returned)==0,"PEB query failed");
   IntPtr parameters=Pointer(child,IntPtr.Add(info[1],0x20));IntPtr console=Pointer(child,IntPtr.Add(parameters,0x10));
   Require(console==IntPtr.Zero,"Child has a console: "+console);evidence.AppendLine("PASS responsive manager window title=EasyRetro; child PEB ConsoleHandle=0; no apply");
   PostMessage(child.MainWindowHandle,0x10,IntPtr.Zero,IntPtr.Zero);Require(child.WaitForExit(10000),"Manager did not close gracefully");Require(child.ExitCode==0,"Manager exit="+child.ExitCode);
   evidence.AppendLine("PASS graceful GUI close exit=0");File.WriteAllText(args[1],evidence.ToString(),new UTF8Encoding(false));return 0;
  }catch(Exception error){evidence.AppendLine(error.ToString());File.WriteAllText(args[1],evidence.ToString(),new UTF8Encoding(false));return 1;}
  finally {if(child!=null){if(!child.HasExited&&child.MainWindowHandle!=IntPtr.Zero)PostMessage(child.MainWindowHandle,0x10,IntPtr.Zero,IntPtr.Zero);child.Dispose();}}
 }
}

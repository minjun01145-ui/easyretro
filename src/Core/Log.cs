using System;
using System.Diagnostics;
using System.IO;
using System.Text;
namespace EasyRetro {
 // GUI and runas workers never access Console streams, including through logging.
 internal static class ConsoleOutput {
  static bool enabled;
  public static void Attach() {
   try {
    if(!NativeMethods.AttachConsole(-1))return;
    if(!NativeMethods.SetConsoleOutputCP(65001))return;
    Console.OutputEncoding=new UTF8Encoding(false);
    enabled=true;
   }catch(IOException){/* A console is optional, even for command-line modes. */}
   catch(UnauthorizedAccessException){/* Console setup must not prevent startup. */}
   catch(System.Security.SecurityException){/* Restricted console access is optional. */}
  }
  public static void WriteLine(string message) {
   if(!enabled)return;
   try{Console.WriteLine(message);}catch(IOException){/* A closed parent console is harmless. */}catch(ObjectDisposedException){/* The parent may exit during work. */}
  }
  public static void WriteError(string message) {
   if(!enabled)return;
   try{Console.Error.WriteLine(message);}catch(IOException){/* Preserve the original error. */}catch(ObjectDisposedException){/* Preserve the original error. */}
  }
 }
 internal static class Log {
  static readonly object Gate=new object(); public static string FilePath;
  public static void Start(StateStore s) {Directory.CreateDirectory(s.PathFor("logs")); FilePath=s.PathFor("logs/easyretro-"+DateTime.Now.ToString("yyyyMMdd")+".log");}
  public static void Write(string message) {
   ConsoleOutput.WriteLine(message);
   lock(Gate){if(FilePath!=null)try{File.AppendAllText(FilePath,DateTime.Now.ToString("o")+" "+message+Environment.NewLine,Encoding.UTF8);}catch(IOException){/* Logging must never interrupt rollback, including a full disk. */}catch(UnauthorizedAccessException){/* Preserve the original operation error. */}}
  }
  public static string Failure(Exception error) {
   if(FilePath==null)try{Start(StateStore.Default());}catch(IOException){FilePath=Path.Combine(Path.GetTempPath(),"easyretro-error.log");}catch(UnauthorizedAccessException){FilePath=Path.Combine(Path.GetTempPath(),"easyretro-error.log");}
   Write(error.ToString());return "작업을 완료하지 못했습니다. 오류 내용을 확인한 후 다시 시도하세요.\n\n"+error.Message+"\n\n로그: "+FilePath;
  }
 }
 internal static class Processes {
  public static string Q(string s) {return "\""+s.Replace("\"","\\\"")+"\"";}
  public static int Run(string file,string args,string workingDirectory=null,params int[] accepted) {
   ProcessStartInfo info=new ProcessStartInfo(file,args){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
   if(workingDirectory!=null)info.WorkingDirectory=workingDirectory;
   using(Process p=Process.Start(info)) {
    // Drain both pipes concurrently: a noisy installer must not deadlock on stderr.
    System.Threading.Tasks.Task<string> output=p.StandardOutput.ReadToEndAsync(),error=p.StandardError.ReadToEndAsync();
    p.WaitForExit();string text=output.Result,errors=error.Result;
    if(text.Length>0)Log.Write(text.Trim());if(errors.Length>0)Log.Write(errors.Trim());
    if(p.ExitCode!=0&&!Array.Exists(accepted,delegate(int code){return code==p.ExitCode;}))throw new InvalidOperationException("외부 프로그램 실행 실패: "+file+" (종료 코드 "+p.ExitCode+") "+errors);
    return p.ExitCode;
   }
  }
 }
}

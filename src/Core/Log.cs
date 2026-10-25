using System;
using System.Diagnostics;
using System.IO;
using System.Text;
namespace EasyRetro {
 internal static class Log {
  static readonly object Gate=new object(); public static string FilePath;
  public static void Start(StateStore s) {Directory.CreateDirectory(s.PathFor("logs")); FilePath=s.PathFor("logs/easyretro-"+DateTime.Now.ToString("yyyyMMdd")+".log");}
  public static void Write(string message) {Console.WriteLine(message);lock(Gate) {if(FilePath!=null) File.AppendAllText(FilePath,DateTime.Now.ToString("o")+" "+message+Environment.NewLine,Encoding.UTF8);}}
 }
 internal static class Processes {
  public static string Q(string s) {return "\""+s.Replace("\"","\\\"")+"\"";}
  public static int Run(string file,string args) {ProcessStartInfo info=new ProcessStartInfo(file,args);info.UseShellExecute=false;info.CreateNoWindow=true;info.RedirectStandardOutput=true;info.RedirectStandardError=true;using(Process p=Process.Start(info)) {string output=p.StandardOutput.ReadToEnd();string error=p.StandardError.ReadToEnd();p.WaitForExit();if(output.Length>0)Log.Write(output.Trim());if(p.ExitCode!=0 && p.ExitCode!=3010 && p.ExitCode!=1605)throw new InvalidOperationException(file+" 종료 코드 "+p.ExitCode+": "+error);return p.ExitCode;}}
 }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
internal static class CliCheck {
 [DllImport("kernel32.dll")] static extern bool AllocConsole();
 [DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
 [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr window,int command);
 static int Main(string[] args) {
  try {
   if(GetConsoleWindow()==IntPtr.Zero){if(!AllocConsole())throw new InvalidOperationException("Console allocation failed");ShowWindow(GetConsoleWindow(),0);}
   string[] commands={"--selftest","--dry-run --apply 98","--dry-run --apply xp","--dry-run --restore","--status","--verify"};
   Directory.CreateDirectory(args[1]);StringBuilder summary=new StringBuilder();
   foreach(string command in commands) {
    ProcessStartInfo info=new ProcessStartInfo(args[0],command){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,StandardOutputEncoding=Encoding.UTF8,StandardErrorEncoding=Encoding.UTF8,CreateNoWindow=true};
    using(Process child=Process.Start(info)) {
     System.Threading.Tasks.Task<string> output=child.StandardOutput.ReadToEndAsync(),error=child.StandardError.ReadToEndAsync();
     child.WaitForExit();string text=output.Result+error.Result;
     File.WriteAllText(Path.Combine(args[1],command.Replace("--","").Replace(' ','-')+".txt"),text,new UTF8Encoding(false));
     summary.AppendLine(command+" => exit "+child.ExitCode+"; captured "+text.Length+" characters");
     if(child.ExitCode!=0)throw new InvalidOperationException(command+" failed: "+text);
    }
   }
   File.WriteAllText(Path.Combine(args[1],"cli-summary.txt"),summary.ToString(),new UTF8Encoding(false));return 0;
  }catch(Exception error){File.WriteAllText(Path.Combine(args[1],"cli-failure.txt"),error.ToString(),new UTF8Encoding(false));return 1;}
 }
}

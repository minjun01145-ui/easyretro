using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
namespace EasyRetro {
 internal static class Program {
  static string Sid {get{return WindowsIdentity.GetCurrent().User.Value;}}
  internal static int Elevate(string action,string theme){
   ProcessStartInfo info=new ProcessStartInfo(Application.ExecutablePath,action+(theme==null?"":" "+theme)+" --user-sid "+Sid){UseShellExecute=true,Verb="runas"};
   try{using(Process child=Process.Start(info)){child.WaitForExit();return child.ExitCode;}}
   catch(System.ComponentModel.Win32Exception error){if(error.NativeErrorCode!=1223)throw;ConsoleOutput.WriteLine("관리자 권한 요청을 취소했습니다. 설정은 변경되지 않았습니다.");return 1223;}
  }
  [STAThread] static int Main(string[] args){bool cli=args.Length>0;try {
   // Only read-only CLI modes attach; GUI/runs elevated by runas have no console.
   if(Array.IndexOf(args,"--user-sid")<0&&(Array.IndexOf(args,"--status")>=0||Array.IndexOf(args,"--dry-run")>=0||Array.IndexOf(args,"--selftest")>=0||Array.IndexOf(args,"--help")>=0||Array.IndexOf(args,"-h")>=0||Array.IndexOf(args,"/?")>=0))ConsoleOutput.Attach();
   NativeMethods.WindowsBuild();Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   AppContext.SetSwitch("Switch.System.Windows.Forms.DoNotSupportDpiChanges",false);
   Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
   Application.ThreadException+=delegate(object sender,ThreadExceptionEventArgs eventArgs){MessageBox.Show(Log.Failure(eventArgs.Exception),"EasyRetro · 오류",MessageBoxButtons.OK,MessageBoxIcon.Error);};
   bool dry=false,self=false,status=false,help=false;string action=null,theme=null,originalSid=null;
   for(int i=0;i<args.Length;i++){switch(args[i]){case "--help":case "-h":case "/?":help=true;break;case "--dry-run":dry=true;break;case "--selftest":self=true;break;case "--status":status=true;break;case "--apply":if(action!=null||i+1>=args.Length)throw new ArgumentException("--apply 98|xp");action=args[i];theme=args[++i];ThemeSpec.Get(theme);break;case "--restore":case "--uninstall":if(action!=null)throw new ArgumentException("한 번에 한 작업만 가능합니다.");action=args[i];break;case "--user-sid":if(i+1>=args.Length)throw new ArgumentException("사용자 SID 누락");originalSid=args[++i];break;default:throw new ArgumentException("지원하지 않는 인자: "+args[i]);}}
   if((self?1:0)+(status?1:0)+(help?1:0)+(action==null?0:1)>1 || dry&&(action==null||action=="--uninstall"))throw new ArgumentException("인자 조합 오류");
   if(help){ConsoleOutput.WriteLine("EasyRetro 0.1.1\n사용법: EasyRetro.exe [--status | --selftest | --dry-run --apply 98|xp | --dry-run --restore]\n실제 변경: --apply 98|xp | --restore | --uninstall (관리자 권한 필요)");return 0;}
   bool mutation=action!=null&&!dry;
   bool elevated=new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
   bool created;
   // A non-elevated CLI launcher must not hold the elevated worker's mutex while waiting for runas.
   string name="Local\\EasyRetro-"+Sid+(mutation?(elevated?"-Action":"-Launcher"):"-Manager");
   using(Mutex mutex=new Mutex(true,name,out created)){if(!created){if(!cli)MessageBox.Show("EasyRetro가 이미 실행 중입니다.","EasyRetro");else ConsoleOutput.WriteLine("EasyRetro가 이미 실행 중입니다.");return 2;}
    try {StateStore store=StateStore.Default();Engine engine=new Engine(store,null);
     if(dry){engine.Plan(theme??"98",action=="--restore");return 0;}if(status){engine.Status();return 0;}if(self){SelfTest();return 0;}
     if(mutation){if(originalSid!=null&&originalSid!=Sid)throw new InvalidOperationException("다른 관리자 계정 자격증명을 사용한 권한 상승은 지원하지 않습니다. 원래 사용자 계정에서 실행하세요.");if(!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))return Elevate(action,theme);using(ProgressForm form=new ProgressForm(action,theme)){Application.Run(form);return form.Result;}}
     using(MainForm form=new MainForm())Application.Run(form);return 0;
    }finally{mutex.ReleaseMutex();}
   }
  }catch(Exception error){string message=Log.Failure(error);if(cli)ConsoleOutput.WriteError(message);if(!cli||Array.IndexOf(args,"--user-sid")>=0)MessageBox.Show(message,"EasyRetro · 오류",MessageBoxButtons.OK,MessageBoxIcon.Error);return 1;}}
  static void SelfTest(){
   ConsoleOutput.WriteLine("OS check OK: Windows 11 x64 build "+NativeMethods.WindowsBuild());
   string folder=Path.Combine(Path.GetTempPath(),"EasyRetro-selftest-"+Guid.NewGuid().ToString("N"));
   AssetGenerator.Generate(Path.Combine(folder,"assets"));ConsoleOutput.WriteLine("Assets: "+Path.Combine(folder,"assets"));
   XpAssets.SelfTest(folder,ConsoleOutput.WriteLine);
   foreach(Dependency d in new[]{Dependencies.EP,Dependencies.OS})Downloader.Fetch(d,Path.Combine(folder,"cache"));
   ConsoleOutput.WriteLine("PASS --selftest: every XP source URL hash-verified; original images assembled; WAV headers verified; both dependencies hash-verified; OS check OK; no system settings changed.");
  }
 }
}

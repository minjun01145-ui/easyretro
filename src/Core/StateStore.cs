using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
namespace EasyRetro {
 public sealed class ComponentState { public bool installedByUs,installPending,installPlanned; public string version,productCode,installer,msi; }
 public sealed class AppState { public int version=1; public bool selfInstallPending; public string appliedTheme,snapshotCreatedUtc,operation; public Dictionary<string,ComponentState> components=new Dictionary<string,ComponentState>(); public List<string> completedModules=new List<string>(); }
 public sealed class StateStore {
  public readonly string Root;
  public StateStore(string root) { Root=root; }
  public static StateStore Default() { return new StateStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"EasyRetro")); }
  public string PathFor(string name) { return Path.Combine(Root,name); }
  public bool Exists { get { return File.Exists(PathFor("state.json")); } }
  public AppState Load() { return Exists ? ReadJson<AppState>(PathFor("state.json")) : null; }
  public void Save(AppState state) { WriteJson(PathFor("state.json"),state); }
  public static T ReadJson<T>(string path) { return new JavaScriptSerializer().Deserialize<T>(File.ReadAllText(path,Encoding.UTF8)); }
  [System.Runtime.InteropServices.DllImport("kernel32.dll",CharSet=System.Runtime.InteropServices.CharSet.Unicode,SetLastError=true)]
  static extern bool MoveFileEx(string existing,string replacement,uint flags);
  public static void WriteJson(string path,object value) {
   Directory.CreateDirectory(Path.GetDirectoryName(path));string tmp=path+".tmp";
   byte[] bytes=new UTF8Encoding(false).GetBytes(new JavaScriptSerializer().Serialize(value));
   using(FileStream stream=new FileStream(tmp,FileMode.Create,FileAccess.Write,FileShare.None,4096,FileOptions.WriteThrough)){stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
   // Same-volume atomic replacement; no delete gap or ReplaceFile metadata-merge failure.
   if(!MoveFileEx(tmp,path,0x1|0x8))throw new IOException("상태 파일을 안전하게 교체하지 못했습니다: "+path,new System.ComponentModel.Win32Exception(System.Runtime.InteropServices.Marshal.GetLastWin32Error()));
  }
  public void CleanTheme() {
   // A durable terminal journal makes interrupted cleanup retryable without deleted backups.
   AppState state=Load();if(state!=null){state.operation="cleaning";Save(state);}
   foreach(string d in new[]{"assets","asset-staging","rollback","backup"}){string p=PathFor(d);if(Directory.Exists(p))DeleteDirectory(p);}
   if(Exists)File.Delete(PathFor("state.json"));
  }
  public static void DeleteDirectory(string path) {
   foreach(string file in Directory.GetFiles(path,"*",SearchOption.AllDirectories)){FileAttributes attributes=File.GetAttributes(file);if((attributes&FileAttributes.ReadOnly)!=0)File.SetAttributes(file,attributes&~FileAttributes.ReadOnly);}
   Directory.Delete(path,true);
  }
 }
}

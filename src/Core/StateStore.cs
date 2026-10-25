using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
namespace EasyRetro {
 public sealed class ComponentState { public bool installedByUs,installPending; public string version,productCode,installer; }
 public sealed class AppState { public int version=1; public string appliedTheme,snapshotCreatedUtc,operation; public Dictionary<string,ComponentState> components=new Dictionary<string,ComponentState>(); public List<string> completedModules=new List<string>(); }
 public sealed class StateStore {
  public readonly string Root;
  public StateStore(string root) { Root=root; }
  public static StateStore Default() { return new StateStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"EasyRetro")); }
  public string PathFor(string name) { return Path.Combine(Root,name); }
  public bool Exists { get { return File.Exists(PathFor("state.json")); } }
  public AppState Load() { return Exists ? ReadJson<AppState>(PathFor("state.json")) : null; }
  public void Save(AppState state) { WriteJson(PathFor("state.json"),state); }
  public static T ReadJson<T>(string path) { return new JavaScriptSerializer().Deserialize<T>(File.ReadAllText(path,Encoding.UTF8)); }
  public static void WriteJson(string path,object value) { Directory.CreateDirectory(Path.GetDirectoryName(path)); string tmp=path+".tmp"; File.WriteAllText(tmp,new JavaScriptSerializer().Serialize(value),new UTF8Encoding(false)); if(File.Exists(path)) File.Replace(tmp,path,null); else File.Move(tmp,path); }
  public void CleanTheme() { foreach(string d in new[]{"assets","backup"}) { string p=PathFor(d); if(Directory.Exists(p)) Directory.Delete(p,true); } if(Exists) File.Delete(PathFor("state.json")); }
 }
}

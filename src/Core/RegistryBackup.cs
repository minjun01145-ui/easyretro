using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Win32;
namespace EasyRetro {
 public sealed class ValueRecord { public string module,hive,key,name,kind,data; public bool existed,keyExisted; public string[] strings; }
 public sealed class KeyRecord { public string module,key,file; public bool existed; }
 public sealed class RegistrySnapshot {
  public List<ValueRecord> values=new List<ValueRecord>();
  public List<KeyRecord> keys=new List<KeyRecord>();
  public List<string> missingKeys=new List<string>();
 }
 public sealed class RegistryBackup {
  public readonly string Folder; public RegistrySnapshot Snapshot;
  public RegistryBackup(string folder,bool load) { Folder=folder; Snapshot=load ? StateStore.ReadJson<RegistrySnapshot>(Path.Combine(folder,"values.json")) : new RegistrySnapshot(); }
  public void Save() { StateStore.WriteJson(Path.Combine(Folder,"values.json"),Snapshot); }
  public void Capture(string module,string key,string name) {
   if(Snapshot.values.Exists(delegate(ValueRecord x){return x.key==key && x.name==name;})) return;
   ValueRecord r=new ValueRecord(); r.module=module;r.hive="HKCU";r.key=key;r.name=name;
   using(RegistryKey k=Registry.CurrentUser.OpenSubKey(key)) { r.keyExisted=k!=null; if(k!=null && Array.Exists(k.GetValueNames(),delegate(string n){return string.Equals(n,name,StringComparison.OrdinalIgnoreCase);})) { r.existed=true; RegistryValueKind kind=k.GetValueKind(name);r.kind=kind.ToString();object data=k.GetValue(name,null,RegistryValueOptions.DoNotExpandEnvironmentNames); if(kind==RegistryValueKind.Binary || kind==RegistryValueKind.None) r.data=Convert.ToBase64String((byte[])data); else if(kind==RegistryValueKind.MultiString) r.strings=(string[])data; else r.data=Convert.ToString(data,CultureInfo.InvariantCulture); } }
   Snapshot.values.Add(r);
   CaptureMissingParents(key);
  }
  public void CaptureKey(string module,string key) {
   if(Snapshot.keys.Exists(delegate(KeyRecord x){return x.key==key;})) return;
   Directory.CreateDirectory(Folder); KeyRecord r=new KeyRecord();r.module=module;r.key=key;r.file="key-"+Snapshot.keys.Count+".reg";
   using(RegistryKey k=Registry.CurrentUser.OpenSubKey(key)) r.existed=k!=null;
   if(r.existed) Processes.Run("reg.exe","export "+Processes.Q("HKCU\\"+key)+" "+Processes.Q(Path.Combine(Folder,r.file))+" /y"); Snapshot.keys.Add(r);
   CaptureMissingParents(key);
  }
  public void Restore(string module) {
   List<Exception> errors=new List<Exception>();
   for(int i=Snapshot.keys.Count-1;i>=0;i--) { KeyRecord r=Snapshot.keys[i];if(r.module!=module)continue;try {Registry.CurrentUser.DeleteSubKeyTree(r.key,false);if(r.existed) Processes.Run("reg.exe","import "+Processes.Q(Path.Combine(Folder,r.file)));}catch(Exception e){errors.Add(e);} }
   for(int i=Snapshot.values.Count-1;i>=0;i--) { ValueRecord r=Snapshot.values[i];if(r.module!=module)continue;try { if(!r.existed) {using(RegistryKey k=Registry.CurrentUser.OpenSubKey(r.key,true)) if(k!=null)k.DeleteValue(r.name,false); if(!r.keyExisted) DeleteEmpty(r.key);continue;} RegistryValueKind kind=(RegistryValueKind)Enum.Parse(typeof(RegistryValueKind),r.kind); object data=r.data; if(kind==RegistryValueKind.Binary || kind==RegistryValueKind.None)data=Convert.FromBase64String(r.data);else if(kind==RegistryValueKind.MultiString)data=r.strings;else if(kind==RegistryValueKind.DWord)data=int.Parse(r.data,CultureInfo.InvariantCulture);else if(kind==RegistryValueKind.QWord)data=long.Parse(r.data,CultureInfo.InvariantCulture); Set(r.key,r.name,data,kind); }catch(Exception e){errors.Add(e);} }
   List<string> missing=new List<string>(Snapshot.missingKeys);
   missing.Sort(delegate(string a,string b){return b.Length.CompareTo(a.Length);});
   foreach(string path in missing)try{DeleteEmpty(path);}catch(Exception e){errors.Add(e);}
   if(errors.Count>0) throw new AggregateException("레지스트리 복구 실패: "+module,errors);
  }
  void CaptureMissingParents(string path) {
   while(path.Length>0) {
    using(RegistryKey k=Registry.CurrentUser.OpenSubKey(path))if(k!=null)break;
    if(!Snapshot.missingKeys.Contains(path))Snapshot.missingKeys.Add(path);
    int split=path.LastIndexOf('\\');
    if(split<0)break;
    path=path.Substring(0,split);
   }
  }
  static void DeleteEmpty(string path) { using(RegistryKey k=Registry.CurrentUser.OpenSubKey(path)) {if(k==null || k.ValueCount!=0 || k.SubKeyCount!=0)return;} Registry.CurrentUser.DeleteSubKey(path,false); }
  public static void Set(string key,string name,object value,RegistryValueKind kind) {using(RegistryKey k=Registry.CurrentUser.CreateSubKey(key)) k.SetValue(name,value,kind);}
 }
}

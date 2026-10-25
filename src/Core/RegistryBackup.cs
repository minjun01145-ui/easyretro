using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Win32;
namespace EasyRetro {
 public sealed class ValueRecord { public string module,hive,key,name,kind,data; public bool existed,keyExisted; public string[] strings; }
 public sealed class KeyRecord { public string module,hive,key,file; public bool existed; }
 public sealed class RegistrySnapshot {
  public List<ValueRecord> values=new List<ValueRecord>();
  public List<KeyRecord> keys=new List<KeyRecord>();
  public List<string> missingKeys=new List<string>();
 }
 public sealed class RegistryBackup {
  public readonly string Folder; public RegistrySnapshot Snapshot;
  public RegistryBackup(string folder,bool load) {
   Folder=folder;Snapshot=load?StateStore.ReadJson<RegistrySnapshot>(Path.Combine(folder,"values.json")):new RegistrySnapshot();
   if(Snapshot==null)throw new InvalidDataException("레지스트리 백업이 비어 있습니다: "+folder);
  }
  static string Hive(string hive){return hive??"HKCU";}
  static RegistryKey Root(string hive){if(Hive(hive)=="HKCU")return Registry.CurrentUser;if(hive=="HKLM")return Registry.LocalMachine;throw new InvalidDataException("지원하지 않는 백업 하이브: "+hive);}
  public void Save() {
   StateStore.WriteJson(Path.Combine(Folder,"values.json"),Snapshot);
   // Close/flush every native/API/export backup before committing the mutation journal.
   foreach(string file in Directory.GetFiles(Folder,"*",SearchOption.AllDirectories)){FileAttributes attributes=File.GetAttributes(file);if((attributes&FileAttributes.ReadOnly)!=0)File.SetAttributes(file,attributes&~FileAttributes.ReadOnly);using(FileStream stream=new FileStream(file,FileMode.Open,FileAccess.ReadWrite,FileShare.Read))stream.Flush(true);}
  }
  public void Capture(string module,string key,string name){Capture(module,"HKCU",key,name);}
  public void Capture(string module,string hive,string key,string name) {
   if(Snapshot.values.Exists(delegate(ValueRecord x){return Hive(x.hive)==hive&&string.Equals(x.key,key,StringComparison.OrdinalIgnoreCase)&&string.Equals(x.name,name,StringComparison.OrdinalIgnoreCase);}))return;
   ValueRecord r=new ValueRecord{module=module,hive=hive,key=key,name=name};
   using(RegistryKey k=Root(hive).OpenSubKey(key)) {
    r.keyExisted=k!=null;
    if(k!=null&&Array.Exists(k.GetValueNames(),delegate(string n){return string.Equals(n,name,StringComparison.OrdinalIgnoreCase);})) {
     r.existed=true;RegistryValueKind kind=k.GetValueKind(name);r.kind=kind.ToString();object data=k.GetValue(name,null,RegistryValueOptions.DoNotExpandEnvironmentNames);
     if(kind==RegistryValueKind.Binary||kind==RegistryValueKind.None)r.data=Convert.ToBase64String((byte[])data);
     else if(kind==RegistryValueKind.MultiString)r.strings=(string[])data;
     else r.data=Convert.ToString(data,CultureInfo.InvariantCulture);
    }
   }
   Snapshot.values.Add(r);CaptureMissingParents(hive,key);
  }
  public void CaptureKey(string module,string key){CaptureKey(module,"HKCU",key);}
  public void CaptureKey(string module,string hive,string key) {
   if(Snapshot.keys.Exists(delegate(KeyRecord x){return Hive(x.hive)==hive&&string.Equals(x.key,key,StringComparison.OrdinalIgnoreCase);}))return;
   Directory.CreateDirectory(Folder);KeyRecord r=new KeyRecord{module=module,hive=hive,key=key,file="key-"+Snapshot.keys.Count+".reg"};
   using(RegistryKey k=Root(hive).OpenSubKey(key))r.existed=k!=null;
   if(r.existed)Processes.Run("reg.exe","export "+Processes.Q(hive+"\\"+key)+" "+Processes.Q(Path.Combine(Folder,r.file))+" /y");
   Snapshot.keys.Add(r);CaptureMissingParents(hive,key);
  }
  public void Restore(string module) {
   List<Exception> errors=new List<Exception>();
   for(int i=Snapshot.keys.Count-1;i>=0;i--) {
    KeyRecord r=Snapshot.keys[i];if(r.module!=module)continue;
    try {
     string file=Path.Combine(Folder,r.file);
     // Never delete a user's current tree before discovering that its backup is missing.
     if(r.existed&&!File.Exists(file))throw new FileNotFoundException("레지스트리 원본 백업 파일이 없습니다.",file);
     Root(r.hive).DeleteSubKeyTree(r.key,false);
     if(r.existed)Processes.Run("reg.exe","import "+Processes.Q(file));
    }catch(Exception e){errors.Add(e);}
   }
   for(int i=Snapshot.values.Count-1;i>=0;i--) {
    ValueRecord r=Snapshot.values[i];if(r.module!=module)continue;
    try {
     if(!r.existed){using(RegistryKey k=Root(r.hive).OpenSubKey(r.key,true))if(k!=null)k.DeleteValue(r.name,false);if(!r.keyExisted)DeleteEmpty(r.hive,r.key);continue;}
     RegistryValueKind kind=(RegistryValueKind)Enum.Parse(typeof(RegistryValueKind),r.kind);object data=r.data;
     if(kind==RegistryValueKind.Binary||kind==RegistryValueKind.None)data=Convert.FromBase64String(r.data);
     else if(kind==RegistryValueKind.MultiString)data=r.strings;
     else if(kind==RegistryValueKind.DWord)data=int.Parse(r.data,CultureInfo.InvariantCulture);
     else if(kind==RegistryValueKind.QWord)data=long.Parse(r.data,CultureInfo.InvariantCulture);
     using(RegistryKey k=Root(r.hive).CreateSubKey(r.key))k.SetValue(r.name,data,kind);
    }catch(Exception e){errors.Add(e);}
   }
   List<string> missing=new List<string>(Snapshot.missingKeys);missing.Sort(delegate(string a,string b){return b.Length.CompareTo(a.Length);});
   foreach(string path in missing)try{bool qualified=path.StartsWith("HKCU\\",StringComparison.Ordinal)||path.StartsWith("HKLM\\",StringComparison.Ordinal);DeleteEmpty(qualified?path.Substring(0,4):"HKCU",qualified?path.Substring(5):path);}catch(Exception e){errors.Add(e);}
   if(errors.Count>0)throw new AggregateException("레지스트리 복구 실패: "+module,errors);
  }
  void CaptureMissingParents(string hive,string path) {
   while(path.Length>0) {
    using(RegistryKey k=Root(hive).OpenSubKey(path))if(k!=null)break;
    string qualified=hive+"\\"+path;if(!Snapshot.missingKeys.Contains(qualified))Snapshot.missingKeys.Add(qualified);
    int split=path.LastIndexOf('\\');if(split<0)break;path=path.Substring(0,split);
   }
  }
  static void DeleteEmpty(string hive,string path){using(RegistryKey k=Root(hive).OpenSubKey(path)){if(k==null||k.ValueCount!=0||k.SubKeyCount!=0)return;}Root(hive).DeleteSubKey(path,false);}
  public static void Set(string key,string name,object value,RegistryValueKind kind){using(RegistryKey k=Registry.CurrentUser.CreateSubKey(key))k.SetValue(name,value,kind);}
 }
}

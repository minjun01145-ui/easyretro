using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using Microsoft.Win32;
namespace EasyRetro {
 public sealed class ValueRecord { public string module,hive,key,name,kind,data; public bool existed,keyExisted; public string[] strings; }
 public sealed class KeyImage { public string path; public List<ValueRecord> values=new List<ValueRecord>(); public List<string> subkeys=new List<string>(); public bool childrenCaptured; }
 public sealed class KeyRecord { public string module,hive,key,file; public bool existed; public List<KeyImage> image; }
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
  public bool HasModule(string module){return Snapshot.values.Exists(delegate(ValueRecord r){return r.module==module;})||Snapshot.keys.Exists(delegate(KeyRecord r){return r.module==module;});}
  public void Save() {
   StateStore.WriteJson(Path.Combine(Folder,"values.json"),Snapshot);
   foreach(string file in Directory.GetFiles(Folder,"*",SearchOption.AllDirectories)){FileAttributes attributes=File.GetAttributes(file);if((attributes&FileAttributes.ReadOnly)!=0)File.SetAttributes(file,attributes&~FileAttributes.ReadOnly);using(FileStream stream=new FileStream(file,FileMode.Open,FileAccess.ReadWrite,FileShare.Read))stream.Flush(true);}
  }
  static ValueRecord ReadValue(RegistryKey key,string module,string hive,string path,string name) {
   ValueRecord r=new ValueRecord{module=module,hive=hive,key=path,name=name,keyExisted=key!=null};
   if(key!=null&&Array.Exists(key.GetValueNames(),delegate(string n){return string.Equals(n,name,StringComparison.OrdinalIgnoreCase);})) {
    r.existed=true;RegistryValueKind kind=key.GetValueKind(name);r.kind=kind.ToString();object data=key.GetValue(name,null,RegistryValueOptions.DoNotExpandEnvironmentNames);
    if(kind==RegistryValueKind.Binary||kind==RegistryValueKind.None)r.data=Convert.ToBase64String((byte[])data);
    else if(kind==RegistryValueKind.MultiString)r.strings=(string[])data;
    else if(kind==RegistryValueKind.Unknown)throw new InvalidDataException("표현할 수 없는 레지스트리 종류: "+hive+"\\"+path+"\\"+name+"; .reg 파일로 수동 복구하세요.");
    else r.data=Convert.ToString(data,CultureInfo.InvariantCulture);
   }
   return r;
  }
  public void Capture(string module,string key,string name){Capture(module,"HKCU",key,name);}
  public void Capture(string module,string hive,string key,string name) {
   if(Snapshot.values.Exists(delegate(ValueRecord x){return Hive(x.hive)==hive&&string.Equals(x.key,key,StringComparison.OrdinalIgnoreCase)&&string.Equals(x.name,name,StringComparison.OrdinalIgnoreCase);}))return;
   using(RegistryKey k=Root(hive).OpenSubKey(key))Snapshot.values.Add(ReadValue(k,module,hive,key,name));
   CaptureMissingParents(hive,key);
  }
  public void CaptureKey(string module,string key){CaptureKey(module,"HKCU",key);}
  public void CaptureKey(string module,string hive,string key) {
   if(Snapshot.keys.Exists(delegate(KeyRecord x){return Hive(x.hive)==hive&&string.Equals(x.key,key,StringComparison.OrdinalIgnoreCase);}))return;
   Directory.CreateDirectory(Folder);KeyRecord r=new KeyRecord{module=module,hive=hive,key=key,file="key-"+Snapshot.keys.Count+".reg",image=new List<KeyImage>()};
   using(RegistryKey k=Root(hive).OpenSubKey(key))r.existed=k!=null;
   if(r.existed) {
    Processes.Run("reg.exe","export "+Processes.Q(hive+"\\"+key)+" "+Processes.Q(Path.Combine(Folder,r.file))+" /y");
    CaptureImage(r,key,0);
   }
   Snapshot.keys.Add(r);CaptureMissingParents(hive,key);
  }
  void CaptureImage(KeyRecord record,string path,int depth) {
   using(RegistryKey key=Root(record.hive).OpenSubKey(path)) {
    if(key==null)throw new IOException("백업 중 레지스트리 키가 사라졌습니다: "+path);
    KeyImage image=new KeyImage{path=path,childrenCaptured=depth<8};
    foreach(string name in key.GetValueNames())image.values.Add(ReadValue(key,record.module,record.hive,path,name));
    image.subkeys.AddRange(key.GetSubKeyNames());record.image.Add(image);
    if(image.childrenCaptured)foreach(string name in image.subkeys)CaptureImage(record,path+"\\"+name,depth+1);
   }
  }
  public static bool Matches(ValueRecord expected) {
   ValueRecord current;using(RegistryKey key=Root(expected.hive).OpenSubKey(expected.key))current=ReadValue(key,expected.module,Hive(expected.hive),expected.key,expected.name);
   if(current.existed!=expected.existed)return false;if(!expected.existed)return true;
   if(current.kind!=expected.kind||current.data!=expected.data)return false;
   if(current.strings==null||expected.strings==null)return current.strings==expected.strings;
   if(current.strings.Length!=expected.strings.Length)return false;
   for(int i=0;i<current.strings.Length;i++)if(current.strings[i]!=expected.strings[i])return false;
   return true;
  }
  static object Data(ValueRecord r) {
   RegistryValueKind kind=(RegistryValueKind)Enum.Parse(typeof(RegistryValueKind),r.kind);
   if(kind==RegistryValueKind.Binary||kind==RegistryValueKind.None)return Convert.FromBase64String(r.data);
   if(kind==RegistryValueKind.MultiString)return r.strings;
   if(kind==RegistryValueKind.DWord)return int.Parse(r.data,CultureInfo.InvariantCulture);
   if(kind==RegistryValueKind.QWord)return long.Parse(r.data,CultureInfo.InvariantCulture);
   return r.data;
  }
  static void Item(string path,Action action,List<Exception> errors,bool tolerateDenied) {
   try{action();}catch(Exception error) {
    int code=error is UnauthorizedAccessException||error is SecurityException?5:error.HResult&0xffff;
    Log.Write((code==5?"복구 건너뜀(권한 없음): ":"레지스트리 복구 실패: ")+path+" (코드 0x"+code.ToString("X8",CultureInfo.InvariantCulture)+") "+error.Message);
    if(code!=2&&code!=3&&!(code==5&&tolerateDenied))errors.Add(error);
   }
  }
  static void RestoreValue(ValueRecord r) {
   if(Matches(r))return;
   if(!r.existed){using(RegistryKey key=Root(r.hive).OpenSubKey(r.key,true))if(key!=null)key.DeleteValue(r.name,false);return;}
   using(RegistryKey key=Root(r.hive).CreateSubKey(r.key))key.SetValue(r.name,Data(r),(RegistryValueKind)Enum.Parse(typeof(RegistryValueKind),r.kind));
  }
  static bool Contains(List<string> names,string value){return names.Exists(delegate(string name){return string.Equals(name,value,StringComparison.OrdinalIgnoreCase);});}
  void RestoreKey(KeyRecord record,List<Exception> errors) {
   string qualified=Hive(record.hive)+"\\"+record.key;
   if(!record.existed){Item(qualified,delegate{using(RegistryKey key=Root(record.hive).OpenSubKey(record.key))if(key==null)return;Root(record.hive).DeleteSubKeyTree(record.key,false);},errors,true);return;}
   if(record.image==null) {
    // Older backups have only an export: import merges, never delete the live tree first.
    Item(qualified,delegate{string file=Path.Combine(Folder,record.file);if(!File.Exists(file))throw new FileNotFoundException("레지스트리 원본 백업 파일이 없습니다.",file);Processes.Run("reg.exe","import "+Processes.Q(file));},errors,false);return;
   }
   foreach(KeyImage node in record.image) {
    KeyImage image=node;string path=Hive(record.hive)+"\\"+image.path;
    Item(path,delegate{using(RegistryKey key=Root(record.hive).OpenSubKey(image.path))if(key!=null)return;using(RegistryKey key=Root(record.hive).CreateSubKey(image.path)){}},errors,false);
    foreach(ValueRecord value in image.values){ValueRecord r=value;Item(path+"\\"+r.name,delegate{RestoreValue(r);},errors,false);}
   }
   // Reconstruct the entire captured tree before removing only uncaptured items.
   for(int i=record.image.Count-1;i>=0;i--) {
    KeyImage image=record.image[i];string path=Hive(record.hive)+"\\"+image.path;
    string[] names=null,subkeys=null;
    Item(path,delegate{using(RegistryKey key=Root(record.hive).OpenSubKey(image.path)){if(key==null)return;names=key.GetValueNames();subkeys=key.GetSubKeyNames();}},errors,false);
    if(names!=null)foreach(string name in names)if(!image.values.Exists(delegate(ValueRecord r){return string.Equals(r.name,name,StringComparison.OrdinalIgnoreCase);})) {
     string extra=name;Item(path+"\\"+extra,delegate{using(RegistryKey key=Root(record.hive).OpenSubKey(image.path,true))if(key!=null)key.DeleteValue(extra,false);},errors,false);
    }
    if(subkeys!=null&&image.childrenCaptured)foreach(string name in subkeys)if(!Contains(image.subkeys,name)) {
     string extra=image.path+"\\"+name;Item(Hive(record.hive)+"\\"+extra,delegate{Root(record.hive).DeleteSubKeyTree(extra,false);},errors,false);
    }
   }
  }
  public void Restore(string module) {
   if(!HasModule(module))return;
   List<Exception> errors=new List<Exception>();
   for(int i=Snapshot.keys.Count-1;i>=0;i--)if(Snapshot.keys[i].module==module)RestoreKey(Snapshot.keys[i],errors);
   for(int i=Snapshot.values.Count-1;i>=0;i--) {
    ValueRecord r=Snapshot.values[i];if(r.module!=module)continue;
    Item(Hive(r.hive)+"\\"+r.key+"\\"+r.name,delegate{RestoreValue(r);if(!r.keyExisted)DeleteEmpty(r.hive,r.key);},errors,false);
   }
   List<string> missing=new List<string>(Snapshot.missingKeys);missing.Sort(delegate(string a,string b){return b.Length.CompareTo(a.Length);});
   foreach(string item in missing) {
    string path=item;bool qualified=path.StartsWith("HKCU\\",StringComparison.Ordinal)||path.StartsWith("HKLM\\",StringComparison.Ordinal);
    string hive=qualified?path.Substring(0,4):"HKCU",key=qualified?path.Substring(5):path;
    bool owned=Snapshot.values.Exists(delegate(ValueRecord r){return r.module==module&&Hive(r.hive)==hive&&(r.key==key||r.key.StartsWith(key+"\\",StringComparison.OrdinalIgnoreCase));})||Snapshot.keys.Exists(delegate(KeyRecord r){return r.module==module&&Hive(r.hive)==hive&&(r.key==key||r.key.StartsWith(key+"\\",StringComparison.OrdinalIgnoreCase));});
    if(owned)Item(path,delegate{DeleteEmpty(hive,key);},errors,true);
   }
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

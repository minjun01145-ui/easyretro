using System;
using System.Collections.Generic;
using System.IO;
namespace EasyRetro {
 public sealed class ComponentFolder {public string component,name;public bool existed;public List<string> files=new List<string>(),directories=new List<string>();}
 internal static class ComponentFiles {
  static string Target(ComponentFolder folder){return Path.Combine(Environment.GetFolderPath(folder.name.EndsWith("-roaming",StringComparison.Ordinal)?Environment.SpecialFolder.ApplicationData:Environment.SpecialFolder.LocalApplicationData),folder.component==Dependencies.EP.Id?"ExplorerPatcher":"OpenShell");}
  public static void Backup(string directory) {
   List<ComponentFolder> folders=new List<ComponentFolder>();
   foreach(Dependency dependency in new[]{Dependencies.EP,Dependencies.OS})if(Dependencies.Detect(dependency)==null)foreach(string suffix in new[]{"-local","-roaming"}) {
    ComponentFolder folder=new ComponentFolder{component=dependency.Id,name=dependency.Id+suffix};string target=Target(folder);folder.existed=Directory.Exists(target);
    if(folder.existed) {
     foreach(string child in Directory.GetDirectories(target,"*",SearchOption.AllDirectories))folder.directories.Add(child.Substring(target.Length+1));
     foreach(string file in Directory.GetFiles(target,"*",SearchOption.AllDirectories)){string relative=file.Substring(target.Length+1);folder.files.Add(relative);string copy=Path.Combine(directory,folder.name,relative);Directory.CreateDirectory(Path.GetDirectoryName(copy));File.Copy(file,copy,true);}
    }
    folders.Add(folder);
   }
   StateStore.WriteJson(Path.Combine(directory,"component-files.json"),folders);
  }
  public static void Restore(string directory,AppState state) {
   List<ComponentFolder> folders=StateStore.ReadJson<List<ComponentFolder>>(Path.Combine(directory,"component-files.json"));List<Exception> errors=new List<Exception>();
   foreach(ComponentFolder folder in folders) {
    ComponentState component;Dependency dependency=folder.component==Dependencies.EP.Id?Dependencies.EP:Dependencies.OS;
    if(!state.components.TryGetValue(folder.component,out component)||!component.installedByUs||Dependencies.Detect(dependency)!=null)continue;
    try {
     bool complete=true;
     foreach(string relative in folder.files)if(!File.Exists(Path.Combine(directory,folder.name,relative))){complete=false;errors.Add(new FileNotFoundException("구성요소 원본 파일 백업이 없습니다.",Path.Combine(directory,folder.name,relative)));}
     string target=Target(folder);
     // A partial backup must never erase the live folder before salvaging the remaining files.
     if(complete&&Directory.Exists(target))StateStore.DeleteDirectory(target);
     if(folder.existed) {
      Directory.CreateDirectory(target);foreach(string child in folder.directories)Directory.CreateDirectory(Path.Combine(target,child));
      foreach(string relative in folder.files) {
       string source=Path.Combine(directory,folder.name,relative);if(!File.Exists(source))continue;
       try{string destination=Path.Combine(target,relative);Directory.CreateDirectory(Path.GetDirectoryName(destination));File.Copy(source,destination,true);}catch(Exception error){errors.Add(error);}
      }
     }
    }catch(Exception error){errors.Add(error);}
   }
   if(errors.Count>0)throw new AggregateException("구성요소 사용자 캐시 복구 실패. 백업을 보존했습니다.",errors);
  }
 }
}

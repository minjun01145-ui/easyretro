using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
namespace EasyRetro {
 // IDesktopWallpaper preserves per-monitor images and slideshow state that SPI alone loses.
 // https://learn.microsoft.com/windows/win32/api/shobjidl_core/nn-shobjidl_core-idesktopwallpaper
 public sealed class WallpaperSnapshot {
  public List<string> monitors=new List<string>(),images=new List<string>(),slideshow=new List<string>(),cachedFiles=new List<string>();
  public uint background,options,interval,status;public int position;public bool cacheExisted;
 }
 internal static class WallpaperState {
  static string Cache {get{return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),@"Microsoft\Windows\Themes");}}
  static IDesktopWallpaper Create(){return (IDesktopWallpaper)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("C2CF3110-460E-4FC1-B9D0-8A1C0C9CC4BD")));}
  public static void Backup(string folder) {
   WallpaperSnapshot snapshot=new WallpaperSnapshot();IDesktopWallpaper desktop=Create();
   try {
    uint count;desktop.GetMonitorDevicePathCount(out count);
    for(uint i=0;i<count;i++){string monitor;desktop.GetMonitorDevicePathAt(i,out monitor);IntPtr image;int result=desktop.GetWallpaper(monitor,out image);Marshal.ThrowExceptionForHR(result);try{snapshot.monitors.Add(monitor);snapshot.images.Add(Marshal.PtrToStringUni(image)??"");}finally{Marshal.FreeCoTaskMem(image);}}
    desktop.GetBackgroundColor(out snapshot.background);desktop.GetPosition(out snapshot.position);desktop.GetStatus(out snapshot.status);
    if((snapshot.status&2)!=0) {
     desktop.GetSlideshowOptions(out snapshot.options,out snapshot.interval);IShellItemArray items;desktop.GetSlideshow(out items);
     try{uint length;items.GetCount(out length);for(uint i=0;i<length;i++){IShellItem item;items.GetItemAt(i,out item);try{IntPtr name;item.GetDisplayName(0x80028000,out name);try{snapshot.slideshow.Add(Marshal.PtrToStringUni(name));}finally{Marshal.FreeCoTaskMem(name);}}finally{Marshal.FinalReleaseComObject(item);}}}
     finally{Marshal.FinalReleaseComObject(items);}
    }
   }finally{Marshal.FinalReleaseComObject(desktop);}
   for(int i=0;i<snapshot.images.Count;i++)if(File.Exists(snapshot.images[i]))File.Copy(snapshot.images[i],Path.Combine(folder,"wallpaper-original-"+i+Path.GetExtension(snapshot.images[i])),true);
   snapshot.cacheExisted=Directory.Exists(Cache);
   if(snapshot.cacheExisted)foreach(string file in Directory.GetFiles(Cache,"*",SearchOption.AllDirectories)){string relative=file.Substring(Cache.Length+1);snapshot.cachedFiles.Add(relative);string target=Path.Combine(folder,"wallpaper-cache",relative);Directory.CreateDirectory(Path.GetDirectoryName(target));File.Copy(file,target,true);}
   StateStore.WriteJson(Path.Combine(folder,"wallpaper-state.json"),snapshot);
  }
  public static void Restore(string folder) {
   WallpaperSnapshot snapshot=StateStore.ReadJson<WallpaperSnapshot>(Path.Combine(folder,"wallpaper-state.json"));IDesktopWallpaper desktop=Create();List<Exception> errors=new List<Exception>();
   try {
    try{desktop.SetPosition(snapshot.position);desktop.SetBackgroundColor(snapshot.background);}catch(Exception error){errors.Add(error);}
    for(int i=0;i<snapshot.monitors.Count;i++)try {
     string image=snapshot.images[i],saved=Path.Combine(folder,"wallpaper-original-"+i+Path.GetExtension(image));
     if(image.Length>0&&!File.Exists(image)&&File.Exists(saved)){Directory.CreateDirectory(Path.GetDirectoryName(image));File.Copy(saved,image);}
     desktop.SetWallpaper(snapshot.monitors[i],image);
    }catch(Exception error){errors.Add(error);}
    if(snapshot.slideshow.Count>0)try {
     IntPtr[] ids=new IntPtr[snapshot.slideshow.Count];IShellItemArray items=null;
     try{for(int i=0;i<ids.Length;i++){uint attributes=0;Marshal.ThrowExceptionForHR(SHParseDisplayName(snapshot.slideshow[i],IntPtr.Zero,out ids[i],0,out attributes));}Marshal.ThrowExceptionForHR(SHCreateShellItemArrayFromIDLists((uint)ids.Length,ids,out items));desktop.SetSlideshow(items);desktop.SetSlideshowOptions(snapshot.options,snapshot.interval);}
     finally{if(items!=null)Marshal.FinalReleaseComObject(items);foreach(IntPtr id in ids)if(id!=IntPtr.Zero)Marshal.FreeCoTaskMem(id);}
    }catch(Exception error){errors.Add(error);}
    try{desktop.Enable((snapshot.status&1)!=0);}catch(Exception error){errors.Add(error);}
   }finally{Marshal.FinalReleaseComObject(desktop);}
   // Restore transcodes after API calls regenerate them. Missing backup files are errors, never
   // reasons to erase the current cache; the journal remains available for another restore.
   foreach(string relative in snapshot.cachedFiles)try{string source=Path.Combine(folder,"wallpaper-cache",relative);if(!File.Exists(source))throw new FileNotFoundException("배경 원본 캐시 백업이 없습니다.",source);string target=Path.Combine(Cache,relative);Directory.CreateDirectory(Path.GetDirectoryName(target));File.Copy(source,target,true);}catch(Exception error){errors.Add(error);}
   if(errors.Count==0&&Directory.Exists(Cache)) {
    foreach(string file in Directory.GetFiles(Cache,"*",SearchOption.AllDirectories))if(!snapshot.cachedFiles.Contains(file.Substring(Cache.Length+1)))File.Delete(file);
    string[] directories=Directory.GetDirectories(Cache,"*",SearchOption.AllDirectories);Array.Sort(directories,delegate(string a,string b){return b.Length.CompareTo(a.Length);});
    foreach(string directory in directories)if(Directory.GetFileSystemEntries(directory).Length==0)Directory.Delete(directory);
    if(!snapshot.cacheExisted&&Directory.GetFileSystemEntries(Cache).Length==0)Directory.Delete(Cache);
   }
   if(errors.Count>0)throw new AggregateException("배경/슬라이드쇼 원본 복구 실패. 백업을 보존했습니다.",errors);
  }
  [DllImport("shell32.dll",CharSet=CharSet.Unicode)] static extern int SHParseDisplayName(string name,IntPtr bind,out IntPtr id,uint attributes,out uint output);
  [DllImport("shell32.dll")] static extern int SHCreateShellItemArrayFromIDLists(uint count,IntPtr[] ids,[MarshalAs(UnmanagedType.Interface)]out IShellItemArray array);
  [ComImport,Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IDesktopWallpaper {
   void SetWallpaper([MarshalAs(UnmanagedType.LPWStr)]string monitor,[MarshalAs(UnmanagedType.LPWStr)]string wallpaper);
   [PreserveSig]int GetWallpaper([MarshalAs(UnmanagedType.LPWStr)]string monitor,out IntPtr wallpaper);
   void GetMonitorDevicePathAt(uint index,[MarshalAs(UnmanagedType.LPWStr)]out string monitor);
   void GetMonitorDevicePathCount(out uint count);
   void GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)]string monitor,IntPtr rectangle);
   void SetBackgroundColor(uint color);void GetBackgroundColor(out uint color);
   void SetPosition(int position);void GetPosition(out int position);
   void SetSlideshow(IShellItemArray items);void GetSlideshow(out IShellItemArray items);
   void SetSlideshowOptions(uint options,uint interval);void GetSlideshowOptions(out uint options,out uint interval);
   void AdvanceSlideshow([MarshalAs(UnmanagedType.LPWStr)]string monitor,int direction);void GetStatus(out uint status);
   void Enable([MarshalAs(UnmanagedType.Bool)]bool enable);
  }
  [ComImport,Guid("B63EA76D-1F85-456F-A19C-48159EFA858B"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IShellItemArray {
   void BindToHandler(IntPtr context,ref Guid handler,ref Guid iid,out IntPtr value);
   void GetPropertyStore(int flags,ref Guid iid,out IntPtr store);
   void GetPropertyDescriptionList(IntPtr key,ref Guid iid,out IntPtr descriptions);
   void GetAttributes(uint flags,uint mask,out uint attributes);void GetCount(out uint count);void GetItemAt(uint index,out IShellItem item);void EnumItems(out IntPtr items);
  }
  [ComImport,Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] interface IShellItem {
   void BindToHandler(IntPtr context,ref Guid handler,ref Guid iid,out IntPtr value);void GetParent(out IShellItem parent);void GetDisplayName(uint form,out IntPtr name);void GetAttributes(uint mask,out uint attributes);void Compare(IShellItem other,uint hint,out int order);
  }
 }
}

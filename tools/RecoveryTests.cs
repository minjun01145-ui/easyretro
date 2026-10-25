using System;
using System.Drawing;
using System.IO;
using System.Net;
using Microsoft.Win32;
namespace EasyRetro {
 internal static class RecoveryTests {
  sealed class DeniedNetwork:IWebRequestCreate {
   public int Calls;
   public WebRequest Create(Uri uri){Calls++;throw new InvalidOperationException("Network forbidden by recovery test: "+uri);}
  }
  static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
  static string TestRoot(string root){Require(root.StartsWith(@"Software\EasyRetroTest",StringComparison.Ordinal),"Only synthetic HKCU test roots are allowed");return root;}
  static void Seed(string path) {
   using(RegistryKey key=Registry.CurrentUser.CreateSubKey(path)) {
    key.SetValue("", "original default",RegistryValueKind.String);
    key.SetValue("String","original 한글",RegistryValueKind.String);
    key.SetValue("ExpandString",@"%USERPROFILE%\literal",RegistryValueKind.ExpandString);
    key.SetValue("MultiString",new[]{"alpha","한글","omega"},RegistryValueKind.MultiString);
    key.SetValue("DWord",unchecked((int)0xfedcba98),RegistryValueKind.DWord);
    key.SetValue("QWord",unchecked((long)0xfedcba9876543210),RegistryValueKind.QWord);
    key.SetValue("Binary",new byte[]{0,1,127,128,255},RegistryValueKind.Binary);
    key.SetValue("None",new byte[]{0,255,2,128},RegistryValueKind.None);
   }
  }
  static void Capture(string folder,string root) {
   Directory.CreateDirectory(folder);string values=root+@"\Values",tree=root+@"\Tree",legacy=root+@"\ExportOnly";
   Seed(values);Seed(tree);Seed(tree+@"\Child\Grandchild");Seed(legacy);
   RegistryBackup backup=new RegistryBackup(folder,false);
   using(RegistryKey key=Registry.CurrentUser.OpenSubKey(values))foreach(string name in key.GetValueNames())backup.Capture("Synthetic",values,name);
   backup.Capture("Synthetic",values,"AbsentValue");backup.Capture("Synthetic",root+@"\AbsentValueKey","missing");
   backup.CaptureKey("Synthetic",tree);backup.CaptureKey("Synthetic",root+@"\AbsentTree");backup.CaptureKey("Synthetic",legacy);
   // Exercise compatibility with old backups that contain only a .reg export.
   backup.Snapshot.keys[backup.Snapshot.keys.Count-1].image=null;backup.Save();
   using(RegistryKey key=Registry.CurrentUser.CreateSubKey(values))foreach(string name in key.GetValueNames())key.SetValue(name,"mutated",RegistryValueKind.String);
   using(RegistryKey key=Registry.CurrentUser.CreateSubKey(values))key.SetValue("AbsentValue","added");
   using(RegistryKey key=Registry.CurrentUser.CreateSubKey(root+@"\AbsentValueKey"))key.SetValue("missing","added");
   Registry.CurrentUser.DeleteSubKeyTree(tree+@"\Child");
   using(RegistryKey key=Registry.CurrentUser.CreateSubKey(tree)){key.SetValue("String","mutated");key.SetValue("Extra",123);}
   using(RegistryKey key=Registry.CurrentUser.CreateSubKey(tree+@"\ExtraChild"))key.SetValue("added",1);
   using(RegistryKey key=Registry.CurrentUser.CreateSubKey(root+@"\AbsentTree\Added"))key.SetValue("added",1);
   using(RegistryKey key=Registry.CurrentUser.CreateSubKey(legacy))key.SetValue("String","mutated");
   Console.WriteLine("PASS synthetic backup: String, ExpandString, MultiString, DWord, QWord, Binary, None, default value, subkeys, absent value/key, .reg exports and legacy export-only key");
  }
  static void Mutated(string root) {
   using(RegistryKey key=Registry.CurrentUser.OpenSubKey(root+@"\Values"))Require((string)key.GetValue("String")=="mutated","Dry run changed a value");
   using(RegistryKey key=Registry.CurrentUser.OpenSubKey(root+@"\AbsentTree\Added"))Require(key!=null,"Dry run removed a key");
   Console.WriteLine("PASS dry-run left synthetic mutations untouched");
  }
  static void Restored(string folder,string root) {
   RegistryBackup backup=new RegistryBackup(folder,true);
   foreach(ValueRecord value in backup.Snapshot.values)Require(RegistryBackup.Matches(value),"Value mismatch: "+value.key+" ["+value.name+"] "+value.kind);
   foreach(KeyRecord record in backup.Snapshot.keys) {
    using(RegistryKey key=Registry.CurrentUser.OpenSubKey(record.key))Require((key!=null)==record.existed,"Key existence mismatch: "+record.key);
    if(record.image==null)continue;
    foreach(KeyImage image in record.image) {
     foreach(ValueRecord value in image.values)Require(RegistryBackup.Matches(value),"Tree value mismatch: "+value.key+" ["+value.name+"]");
     using(RegistryKey key=Registry.CurrentUser.OpenSubKey(image.path)){Require(key.ValueCount==image.values.Count,"Extra tree value remains");if(image.childrenCaptured)Require(key.SubKeyCount==image.subkeys.Count,"Extra/missing subkey");}
    }
   }
   using(RegistryKey key=Registry.CurrentUser.OpenSubKey(root+@"\ExportOnly"))Require((string)key.GetValue("String")=="original 한글","Legacy export was not imported");
   using(RegistryKey key=Registry.CurrentUser.OpenSubKey(root+@"\AbsentValueKey"))Require(key==null,"Absent-at-capture value key remains");
   Console.WriteLine("PASS exact round-trip: all seven value kinds, default value, subkeys, absent-at-capture keys, .reg import, no extra tree values/subkeys");
  }
  static void FakeRemove(string[] args) {
   string root=TestRoot(Environment.GetEnvironmentVariable("EASYRETRO_TEST_UNINSTALL_KEY"));
   string marker=Environment.GetEnvironmentVariable("EASYRETRO_TEST_UNINSTALL_MARKER");
   Registry.CurrentUser.DeleteSubKeyTree(root,false);File.WriteAllText(marker,string.Join(" ",args));
  }
  static void Uninstall(string folder,string root) {
   DeniedNetwork network=new DeniedNetwork();Require(WebRequest.RegisterPrefix("http://192.0.2.1/",network),"Could not register network sentinel");
   string priorUrl=Dependencies.EP.Url,priorRegistry=Dependencies.EP.RegistryPath;
   try {
    Dependencies.EP.Url="http://192.0.2.1/ep_setup.exe";Dependencies.EP.RegistryPath=root+@"\Uninstall";
    foreach(bool bare in new[]{false,true}) {
     string entry=Dependencies.EP.RegistryPath+@"\ExplorerPatcher-Synthetic",marker=Path.Combine(folder,bare?"silent-marker.txt":"verbatim-marker.txt");
     using(RegistryKey key=Registry.CurrentUser.CreateSubKey(entry)) {
      key.SetValue("DisplayName","ExplorerPatcher synthetic harmless uninstaller");
      key.SetValue(bare?"QuietUninstallString":"UninstallString",Processes.Q(System.Reflection.Assembly.GetExecutingAssembly().Location)+(bare?"":" fake-remove --verbatim"));
     }
     Environment.SetEnvironmentVariable("EASYRETRO_TEST_UNINSTALL_KEY",entry);Environment.SetEnvironmentVariable("EASYRETRO_TEST_UNINSTALL_MARKER",marker);
     AppState state=new AppState();state.components[Dependencies.EP.Id]=new ComponentState{installedByUs=true};
     StateStore store=new StateStore(Path.Combine(folder,bare?"silent-state":"verbatim-state"));
     string warning=Dependencies.Remove(Dependencies.EP,state,store);
     Require(warning==null,"Unexpected leftover warning: "+warning);Require(File.Exists(marker),"Registered uninstaller did not run");
     Require(File.ReadAllText(marker)==(bare?"/uninstall_silent":"fake-remove --verbatim"),"Command switches were not preserved/defaulted");
     Require(network.Calls==0,"A WebRequest was attempted");
     Console.WriteLine("PASS Dependencies.Remove registered "+(bare?"QuietUninstallString + /uninstall_silent":"UninstallString verbatim")+"; EP.Url=http://192.0.2.1/ep_setup.exe; WebRequest calls="+network.Calls);
    }
    string missing=Path.Combine(folder,"missing.exe");
    using(RegistryKey key=Registry.CurrentUser.CreateSubKey(Dependencies.EP.RegistryPath+@"\Leftover")){key.SetValue("DisplayName","ExplorerPatcher synthetic leftover");key.SetValue("UninstallString",Processes.Q(missing));}
    AppState leftover=new AppState();leftover.components[Dependencies.EP.Id]=new ComponentState{installedByUs=true};
    string oldOffline=Downloader.OfflineDirectory;Downloader.OfflineDirectory=Path.Combine(folder,"empty-offline");
    try {string warning=Dependencies.Remove(Dependencies.EP,leftover,new StateStore(Path.Combine(folder,"leftover-state")));Require(warning!=null&&warning.Contains(missing),"Missing offline component must return exact leftover warning");Require(network.Calls==0,"Leftover attempted network in offline mode");}
    finally {Downloader.OfflineDirectory=oldOffline;}
    Console.WriteLine("PASS missing registered installer + empty offline folder returns leftover warning instead of throwing; WebRequest calls=0");
   } finally {Dependencies.EP.Url=priorUrl;Dependencies.EP.RegistryPath=priorRegistry;Environment.SetEnvironmentVariable("EASYRETRO_TEST_UNINSTALL_KEY",null);Environment.SetEnvironmentVariable("EASYRETRO_TEST_UNINSTALL_MARKER",null);}
  }
  static void Assets(string folder) {
   AssetGenerator.Generate(folder);
   using(Bitmap texture=new Bitmap(Path.Combine(folder,"taskbar-98.png"))) {
    Require(texture.Width==50&&texture.Height==32,"98 tile dimensions");
    Require(texture.GetPixel(20,0).ToArgb()==Color.White.ToArgb()&&texture.GetPixel(20,31).ToArgb()==Color.Gray.ToArgb(),"98 bevel rows");
    Require(texture.GetPixel(20,15).ToArgb()==Color.Silver.ToArgb(),"98 texture must be flat grey");
   }
   using(Bitmap start=new Bitmap(Path.Combine(folder,"start-98.png")))Require(start.Width==108&&start.Height==3*32,"98 three-state strip");
   using(PreparedXpAssets xp=XpAssets.Prepare(StateStore.Default().PathFor("cache"),Console.WriteLine)) {
    Require(xp.Substitutes.Count==0,"Original XP assets unavailable for quality test");
    using(Bitmap texture=new Bitmap(Path.Combine(xp.Folder,"taskbar-xp.png"))) {
     Require(texture.Width==50&&texture.Height==48,"XP tile dimensions");
     for(int y=28;y<48;y++)for(int x=0;x<50;x++)Require(texture.GetPixel(x,y)==texture.GetPixel(x,27),"XP bottom row must repeat without interpolation");
    }
    using(Bitmap start=new Bitmap(Path.Combine(xp.Folder,"start-xp.png")))Require(start.Width==100&&start.Height==3*30,"XP three-state strip");
   }
   Console.WriteLine("PASS visual assets: 98 flat 50x32 tile, 108x96 button; XP 50x48 tile repeats bottom row without interpolation, 100x90 button");
  }
  static void DamageIndex(string folder,string root) {
   File.Delete(Path.Combine(folder,"values.json"));
   foreach(string suffix in new[]{@"\Tree",@"\ExportOnly"})using(RegistryKey key=Registry.CurrentUser.OpenSubKey(root+suffix,true))key.SetValue("String","mutated without index");
   Console.WriteLine("Synthetic partial backup: values.json deleted; .reg exports retained and live keys changed");
  }
  static void Salvaged(string root) {
   foreach(string suffix in new[]{@"\Tree",@"\ExportOnly"})using(RegistryKey key=Registry.CurrentUser.OpenSubKey(root+suffix))Require((string)key.GetValue("String")=="original 한글","Remaining export not salvaged: "+suffix);
   Console.WriteLine("PASS missing values.json reported while all remaining .reg exports restored");
  }
  static void ExportRestore(string folder,string root) {
   using(RegistryKey key=Registry.CurrentUser.OpenSubKey(root+@"\Tree",true))key.SetValue("String","mutated app fallback");
   new RegistryBackup(folder,false).RestoreExports();Salvaged(root);
   Console.WriteLine("PASS app RegistryBackup.RestoreExports recovers synthetic keys without the JSON index");
  }
  public static int Main(string[] args) {
   try {
    if(args.Length>0&&(args[0]=="fake-remove"||args[0]=="/uninstall_silent")){FakeRemove(args);return 0;}
    string mode=args[0],folder=args[1],root=TestRoot(args[2]);
    if(mode=="capture")Capture(folder,root);else if(mode=="mutated")Mutated(root);else if(mode=="restored")Restored(folder,root);else if(mode=="uninstall")Uninstall(folder,root);else if(mode=="assets")Assets(folder);else if(mode=="damage-index")DamageIndex(folder,root);else if(mode=="salvaged")Salvaged(root);else if(mode=="export-restore")ExportRestore(folder,root);else throw new ArgumentException(mode);
    return 0;
   }catch(Exception error){Console.Error.WriteLine(error);return 1;}
  }
 }
}

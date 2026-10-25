using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
namespace EasyRetro {
 internal sealed class XpSound {
  public string Name;public int Fallback;public string[] Events;
  public XpSound(string name,int fallback,params string[] events){Name="Windows XP "+name+".wav";Fallback=fallback;Events=events;}
 }
 internal sealed class PreparedXpAssets:IDisposable {
  public string Folder;public readonly List<string> Substitutes=new List<string>();
  public string Notice {get{return Substitutes.Count==0?"":"일부 원본을 받지 못해 자체 생성 에셋으로 대체했습니다: "+string.Join(", ",Substitutes.ToArray());}}
  public void Dispose(){try{if(Directory.Exists(Folder))Directory.Delete(Folder,true);}catch(IOException error){Log.Write("XP 준비 폴더 정리 보류: "+error.Message);}catch(UnauthorizedAccessException error){Log.Write("XP 준비 폴더 정리 보류: "+error.Message);}}
 }
 internal static class XpAssets {
  // Microsoft-copyrighted originals; never embed these files in the repository or executable.
  // Item page: https://archive.org/details/windows-xp-wallpapers_202310 (release bliss.jpg, 800x600).
  public static readonly PinnedArtifact Bliss=new PinnedArtifact{Id="xp-bliss",FileName="xp-bliss.jpg",Size=54607,Sha256="eee00f376a6ff19c32e8670bf630bf8336f11247dd5ce74fb8cd91e5ca33327a",Urls=new[]{"https://archive.org/download/windows-xp-wallpapers_202310/bliss.jpg","https://ia801609.us.archive.org/22/items/windows-xp-wallpapers_202310/bliss.jpg"}};
  // Item page: https://archive.org/details/windows-xp-sounds (unmodified XP Media WAVs in ZIP).
  public static readonly PinnedArtifact Media=new PinnedArtifact{Id="xp-media",FileName="xp-media.zip",Size=8118271,Sha256="3259f9b70db0e1bf5b9b72ddf3736397adabf45569e02377e1882772ed3d8afd",Urls=new[]{"https://archive.org/download/windows-xp-sounds/Windows%20XP%20Sounds.zip","https://ia800601.us.archive.org/22/items/windows-xp-sounds/Windows%20XP%20Sounds.zip"}};
  // Item page: https://archive.org/details/luna_20240122 (ripped from Windows XP).
  // BLUE_TASKBARBACKGROUND_BMP is the original indexed 50x28 blue Luna strip.
  public static readonly PinnedArtifact Luna=new PinnedArtifact{Id="xp-luna",FileName="xp-luna.msstyles",Size=4190352,Sha256="0dbeb9efb7165dc6d812b373b895b7cc84f86a674d0b13ee5e4e7d2ec4a6a852",Urls=new[]{"https://archive.org/download/luna_20240122/luna.msstyles","https://ia800807.us.archive.org/12/items/luna_20240122/luna.msstyles"}};
  // Item page: https://www.classicshell.net/forum/viewtopic.php?t=1152
  // troy2000me captured the three actual XP states; Jcee/Gaurav corrected transparency/ClearType.
  // Attachments 550 and 549 are byte-identical copies, not a fan-redrawn button.
  // Luna's own BLUE_STARTBUTTON_BMP has no logo/text, so use this original rendered screenshot.
  public static readonly PinnedArtifact Start=new PinnedArtifact{Id="xp-start",FileName="xp-start.png",Size=19855,Sha256="246c153ea4489a9dda5a28e2941ffbcd5fec80395963ea07ceaac567170b5dea",Urls=new[]{"https://www.classicshell.net/forum/download/file.php?id=550","https://www.classicshell.net/forum/download/file.php?id=549"}};
  public static readonly PinnedArtifact[] Sources={Bliss,Media,Start,Luna};
  // AppEvents names follow the Windows .theme sound contract. SystemStart/SystemExit and
  // WindowsLogon/Logoff are separate legacy events; modern Windows may suppress playback.
  // AppEvents .Default is the application, while its .Default child is the default beep event.
  public static readonly XpSound[] Sounds={
   new XpSound("Startup",0,".Default\\SystemStart"),
   new XpSound("Logon Sound",0,".Default\\WindowsLogon"),
   new XpSound("Logoff Sound",1,".Default\\WindowsLogoff"),
   new XpSound("Shutdown",1,".Default\\SystemExit"),
   new XpSound("Ding",2,".Default\\SystemAsterisk",".Default\\SystemQuestion"),
   new XpSound("Error",3,".Default\\AppGPFault"),
   new XpSound("Critical Stop",3,".Default\\SystemHand"),
   new XpSound("Exclamation",3,".Default\\SystemExclamation"),
   new XpSound("Notify",4,".Default\\MailBeep",".Default\\Notification.Default"),
   new XpSound("Recycle",5,"Explorer\\EmptyRecycleBin"),
   new XpSound("Balloon",4,".Default\\SystemNotification"),
   new XpSound("Default",2,".Default\\.Default"),
   new XpSound("Minimize",2,".Default\\Minimize"),
   new XpSound("Restore",2,".Default\\RestoreUp",".Default\\RestoreDown",".Default\\Maximize"),
   new XpSound("Menu Command",2,".Default\\MenuCommand"),
   new XpSound("Hardware Insert",4,".Default\\DeviceConnect"),
   new XpSound("Hardware Remove",1,".Default\\DeviceDisconnect"),
   new XpSound("Hardware Fail",3,".Default\\DeviceFail"),
   new XpSound("Battery Low",4,".Default\\LowBatteryAlarm"),
   new XpSound("Battery Critical",3,".Default\\CriticalBatteryAlarm"),
   new XpSound("Print complete",4,".Default\\PrintComplete"),
   new XpSound("Start",2,"Explorer\\Navigating")
  };
  public static IEnumerable<string> Plan(string assets) {
   foreach(PinnedArtifact artifact in Sources) {
    string target=artifact==Bliss?"wallpaper-xp.png":artifact==Start?"start-xp.png":artifact==Luna?"taskbar-xp.png":"Windows XP *.wav";
    foreach(string url in artifact.Urls)yield return artifact.Id+" / "+url+" / SHA-256 "+artifact.Sha256+" → "+Path.Combine(assets,target);
   }
   foreach(XpSound sound in Sounds)yield return Media.Urls[0]+"!/Windows XP Sounds/"+sound.Name+" → "+Path.Combine(assets,sound.Name)+" ("+string.Join(", ",sound.Events)+")";
   yield return "모든 원본 실패 시 해당 항목만 자체 생성 대체; 완료창에 대체 목록 표시";
  }
  static bool Recoverable(Exception error) {return error is IOException||error is InvalidDataException||error is UnauthorizedAccessException||error is InvalidOperationException||error is AggregateException||error is ArgumentException||error is ExternalException;}
  static void PrepareItem(PreparedXpAssets prepared,string name,Action original,Action fallback,Action<string> report) {
   try{original();}catch(Exception error){if(!Recoverable(error))throw;string message="XP 원본 준비 실패 ("+name+"): "+error.Message+"; 이 항목만 자체 생성 대체";Log.Write(message);if(report!=null)report(message);fallback();prepared.Substitutes.Add(name);}
  }
  public static PreparedXpAssets Prepare(string cache,Action<string> report) {
   PreparedXpAssets prepared=new PreparedXpAssets{Folder=Path.Combine(cache,"xp-prepared")};
   if(Directory.Exists(prepared.Folder))Directory.Delete(prepared.Folder,true);Directory.CreateDirectory(prepared.Folder);
   string folder=prepared.Folder;
   try {
    PrepareItem(prepared,"Bliss",delegate{using(Image image=Image.FromFile(Downloader.Fetch(Bliss,cache))){if(image.Width!=800||image.Height!=600)throw new InvalidDataException("Bliss 크기 불일치");image.Save(Path.Combine(folder,"wallpaper-xp.png"),ImageFormat.Png);}},delegate{using(Bitmap image=AssetGenerator.Wallpaper(800,600))image.Save(Path.Combine(folder,"wallpaper-xp.png"),ImageFormat.Png);},report);
    PrepareItem(prepared,"Luna 시작버튼",delegate{AssembleStart(Downloader.Fetch(Start,cache),Path.Combine(folder,"start-xp.png"));},delegate{using(Bitmap image=AssetGenerator.Button(ThemeSpec.Get("xp")))image.Save(Path.Combine(folder,"start-xp.png"),ImageFormat.Png);},report);
    PrepareItem(prepared,"Luna 작업표시줄",delegate{ExtractTaskbar(Downloader.Fetch(Luna,cache),Path.Combine(folder,"taskbar-xp.png"));},delegate{using(Bitmap image=AssetGenerator.Texture(ThemeSpec.Get("xp")))image.Save(Path.Combine(folder,"taskbar-xp.png"),ImageFormat.Png);},report);
    string zip=null;
    try{zip=Downloader.Fetch(Media,cache);}catch(Exception error){if(!Recoverable(error))throw;Log.Write("XP Media 다운로드 실패: "+error.Message);}
    using(ZipArchive archive=zip==null?null:ZipFile.OpenRead(zip))foreach(XpSound sound in Sounds) {
     XpSound item=sound;
     PrepareItem(prepared,item.Name,delegate{if(archive==null)throw new InvalidDataException("XP Media 원본 없음");ZipArchiveEntry entry=archive.GetEntry("Windows XP Sounds/"+item.Name);if(entry==null)throw new InvalidDataException("ZIP 항목 없음: "+item.Name);string target=Path.Combine(folder,item.Name);using(Stream input=entry.Open())using(FileStream output=File.Create(target))input.CopyTo(output);ValidateWave(target);},delegate{AssetGenerator.Wave(Path.Combine(folder,item.Name),item.Fallback);},report);
    }
    return prepared;
   }catch{Directory.Delete(folder,true);throw;}
  }
  public static void Publish(PreparedXpAssets prepared,string assets) {
   Directory.CreateDirectory(assets);foreach(string file in Directory.GetFiles(prepared.Folder))File.Copy(file,Path.Combine(assets,Path.GetFileName(file)),true);
  }
  public static void Backup(string assets,string snapshot) {
   if(!Directory.Exists(assets))return;string target=Path.Combine(snapshot,"assets-previous");Directory.CreateDirectory(target);
   foreach(string file in Directory.GetFiles(assets))File.Copy(file,Path.Combine(target,Path.GetFileName(file)),true);
  }
  public static void Rollback(string assets,string snapshot) {
   string source=Path.Combine(snapshot,"assets-previous");if(Directory.Exists(assets))Directory.Delete(assets,true);
   if(!Directory.Exists(source))return;Directory.CreateDirectory(assets);foreach(string file in Directory.GetFiles(source))File.Copy(file,Path.Combine(assets,Path.GetFileName(file)),true);
  }
  public static void AssembleStart(string source,string target) {
   using(Bitmap image=new Bitmap(source)) {
    if(image.Width!=100||image.Height!=162)throw new InvalidDataException("XP 시작버튼 스트립 크기 불일치");
    // Source: three 100x54 frames, each with 12 transparent rows above/below 30 XP rows.
    // https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/StartMenu/StartMenuDLL/StartButton.cpp#L587-L589
    // normal/hot/pressed stacked equally; caption is in the source pixels, no DPI resampling here.
    using(Bitmap output=new Bitmap(100,90,PixelFormat.Format32bppArgb)) {
     BitmapData input=image.LockBits(new Rectangle(0,0,100,162),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
     try {
      BitmapData destination=output.LockBits(new Rectangle(0,0,100,90),ImageLockMode.WriteOnly,PixelFormat.Format32bppArgb);
      try {
       // Copy raw BGRA rows, not Graphics.DrawImage (which rounds semitransparent colors).
       for(int state=0;state<3;state++)for(int row=0;row<30;row++)
        CopyMemory(IntPtr.Add(destination.Scan0,(state*30+row)*destination.Stride),IntPtr.Add(input.Scan0,(state*54+12+row)*input.Stride),new UIntPtr(400));
      }finally{output.UnlockBits(destination);}
     }finally{image.UnlockBits(input);}
     output.Save(target,ImageFormat.Png);
    }
   }
  }
  [DllImport("kernel32.dll",EntryPoint="RtlMoveMemory")]static extern void CopyMemory(IntPtr destination,IntPtr source,UIntPtr size);
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr LoadLibraryEx(string file,IntPtr reserved,uint flags);
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr FindResource(IntPtr module,string name,IntPtr type);
  [DllImport("kernel32.dll",SetLastError=true)]static extern IntPtr LoadResource(IntPtr module,IntPtr resource);
  [DllImport("kernel32.dll")]static extern IntPtr LockResource(IntPtr resource);
  [DllImport("kernel32.dll",SetLastError=true)]static extern uint SizeofResource(IntPtr module,IntPtr resource);
  [DllImport("kernel32.dll")]static extern bool FreeLibrary(IntPtr module);
  public static void ExtractTaskbar(string source,string target) {
   // LOAD_LIBRARY_AS_DATAFILE only: never execute or install the downloaded msstyles.
   IntPtr module=LoadLibraryEx(source,IntPtr.Zero,2);if(module==IntPtr.Zero)throw new InvalidDataException("Luna 리소스 파일을 열 수 없습니다.");
   try {
    IntPtr resource=FindResource(module,"BLUE_TASKBARBACKGROUND_BMP",new IntPtr(2));if(resource==IntPtr.Zero)throw new InvalidDataException("Luna 작업표시줄 리소스 없음");
    uint size=SizeofResource(module,resource);IntPtr bytes=LockResource(LoadResource(module,resource));if(bytes==IntPtr.Zero||size<40||size>100000)throw new InvalidDataException("Luna 비트맵 리소스 손상");
    byte[] dib=new byte[(int)size];Marshal.Copy(bytes,dib,0,dib.Length);
    if(BitConverter.ToInt32(dib,0)!=40||BitConverter.ToInt32(dib,4)!=50||BitConverter.ToInt32(dib,8)!=28||BitConverter.ToInt16(dib,14)!=8)throw new InvalidDataException("Luna 비트맵 형식 불일치");
    int colors=BitConverter.ToInt32(dib,32);if(colors==0)colors=256;
    using(MemoryStream stream=new MemoryStream()) {
     using(BinaryWriter writer=new BinaryWriter(stream,Encoding.ASCII,true)){writer.Write((ushort)0x4d42);writer.Write(14+dib.Length);writer.Write(0);writer.Write(14+40+colors*4);writer.Write(dib);}
     stream.Position=0;using(Bitmap image=new Bitmap(stream))using(Bitmap output=new Bitmap(50,48,PixelFormat.Format32bppArgb)) {
      // The pinned resource is 50x28, not 28x50. Preserve all 28 rows 1:1, then extend
      // its final row to the 48px taskbar; horizontal tiling introduces no interpolation.
      for(int y=0;y<output.Height;y++)for(int x=0;x<output.Width;x++)output.SetPixel(x,y,image.GetPixel(x,Math.Min(y,image.Height-1)));
      output.Save(target,ImageFormat.Png);
     }
    }
   }finally{FreeLibrary(module);}
  }
  public static string ValidateWave(string path) {
   using(BinaryReader reader=new BinaryReader(File.OpenRead(path))) {
    if(new string(reader.ReadChars(4))!="RIFF")throw new InvalidDataException("WAV RIFF 없음: "+path);uint riff=reader.ReadUInt32();
    if(new string(reader.ReadChars(4))!="WAVE"||riff+8L!=reader.BaseStream.Length)throw new InvalidDataException("WAV 길이/헤더 오류: "+path);
    string format=null;bool data=false;
    while(reader.BaseStream.Position+8<=reader.BaseStream.Length) {
     string tag=new string(reader.ReadChars(4));uint size=reader.ReadUInt32();long end=reader.BaseStream.Position+size;
     if(end>reader.BaseStream.Length)throw new InvalidDataException("WAV 청크 길이 오류: "+path);
     if(tag=="fmt ") {
      if(size<16)throw new InvalidDataException("WAV fmt 오류: "+path);
      ushort code=reader.ReadUInt16(),channels=reader.ReadUInt16();uint rate=reader.ReadUInt32();reader.ReadUInt32();reader.ReadUInt16();ushort bits=reader.ReadUInt16();
      if((code!=1&&code!=2&&code!=17)||channels<1||channels>2||rate==0)throw new InvalidDataException("Windows 비호환 WAV 형식: "+path);
      format=(code==1?"PCM":code==2?"MS ADPCM":"IMA ADPCM")+" / "+channels+"ch / "+rate+" Hz / "+bits+" bit";
     }
     if(tag=="data"&&size>0)data=true;reader.BaseStream.Position=end+(size&1);
    }
    if(format==null||!data)throw new InvalidDataException("WAV fmt/data 없음: "+path);return format;
   }
  }
  public static void SelfTest(string folder,Action<string> report) {
   List<Exception> errors=new List<Exception>();string cache=Path.Combine(folder,"cache");
   foreach(PinnedArtifact artifact in Sources)for(int i=0;i<artifact.Urls.Length;i++) {
    string url=artifact.Urls[i],path=Path.Combine(cache,artifact.Id+"-source-"+i+Path.GetExtension(artifact.FileName));
    try{Downloader.FetchSource(artifact,url,path);report("PASS XP URL "+url+" / SHA-256 "+artifact.Sha256+" / "+new FileInfo(path).Length+" bytes");if(i==0)File.Copy(path,Path.Combine(cache,artifact.FileName),true);}
    catch(Exception error){errors.Add(error);report("FAIL XP URL "+url+" / "+error.Message);}
   }
   if(errors.Count>0)throw new AggregateException("XP 원본 URL 자체검사 실패",errors);
   PreparedXpAssets prepared=Prepare(cache,report);if(prepared.Substitutes.Count!=0)throw new InvalidDataException("자체검사에서는 대체 에셋을 허용하지 않습니다: "+prepared.Notice);
   Publish(prepared,Path.Combine(folder,"assets"));Directory.Delete(prepared.Folder,true);
   foreach(XpSound sound in Sounds)report("PASS WAV "+sound.Name+" / "+ValidateWave(Path.Combine(folder,"assets",sound.Name)));
   report("PASS XP images: Bliss 800x600; start 100x90 (3 x 100x30); taskbar 50x48 (28 native rows + repeated bottom row) / "+Path.Combine(folder,"assets"));
  }
 }
}

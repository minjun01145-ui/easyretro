using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
namespace EasyRetro {
 internal static class NativeMethods {
  [DllImport("ntdll.dll", CharSet=CharSet.Unicode)] static extern int RtlGetVersion(ref OSVERSIONINFO v);
  [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)] struct OSVERSIONINFO { public int size,major,minor,build,platform; [MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)] public string service; }
  [DllImport("kernel32.dll",SetLastError=true)] static extern bool IsWow64Process2(IntPtr process,out ushort processMachine,out ushort nativeMachine);
  static int checkedBuild;
  public static int WindowsBuild() {
   if(checkedBuild!=0)return checkedBuild;
   OSVERSIONINFO v=new OSVERSIONINFO();v.size=Marshal.SizeOf(v);
   if(RtlGetVersion(ref v)!=0 || v.major<10 || v.build<22000 || !Environment.Is64BitOperatingSystem)
    throw new PlatformNotSupportedException("Windows 11 x64 (빌드 22000 이상)이 필요합니다.");
   ushort processMachine,nativeMachine;
   Check(IsWow64Process2(new IntPtr(-1),out processMachine,out nativeMachine));
   if(nativeMachine!=0x8664)throw new PlatformNotSupportedException("이 버전은 x64 전용입니다. ARM64 에뮬레이션은 지원하지 않습니다.");
   checkedBuild=v.build;
   return checkedBuild;
  }
  [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool SystemParametersInfo(uint action,uint param,IntPtr data,uint flags);
  [DllImport("user32.dll",EntryPoint="SystemParametersInfoW",CharSet=CharSet.Unicode,SetLastError=true)] public static extern bool SpiString(uint action,uint param,string data,uint flags);
  [DllImport("user32.dll")] public static extern bool SetSysColors(int count,int[] indexes,int[] colors);
  [DllImport("user32.dll")] public static extern int GetSysColor(int index);
  [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern IntPtr FindWindow(string cls,string title);
  [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hwnd,uint msg,IntPtr w,IntPtr l);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint pid);
  [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern IntPtr SendMessageTimeout(IntPtr hwnd,uint msg,IntPtr w,string l,uint flags,uint timeout,out IntPtr result);
  [DllImport("kernel32.dll")] public static extern bool AttachConsole(int pid);
  [DllImport("kernel32.dll")] public static extern bool AllocConsole();
  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] public struct LOGFONT { public int height,width,escapement,orientation,weight; public byte italic,underline,strikeOut,charSet,outPrecision,clipPrecision,quality,pitch; [MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)] public string face; }
  [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] public struct NONCLIENTMETRICS { public int size,border,scrollWidth,scrollHeight,captionWidth,captionHeight; public LOGFONT caption; public int smallCaptionWidth,smallCaptionHeight; public LOGFONT smallCaption; public int menuWidth,menuHeight; public LOGFONT menu,status,message; public int paddedBorder; }
  public static byte[] SpiGet(uint action,int size) {
   IntPtr p=Marshal.AllocHGlobal(size);
   try {
    byte[] bytes=new byte[size];
    Marshal.Copy(bytes,0,p,size);
    if(action==0x29) Marshal.WriteInt32(p,size);
    // SPI_GETDESKWALLPAPER counts UTF-16 characters; metrics/font operations count bytes.
    uint parameter=(uint)(action==0x73?size/2:size);
    Check(SystemParametersInfo(action,parameter,p,0));
    Marshal.Copy(p,bytes,0,size);
    return bytes;
   } finally {Marshal.FreeHGlobal(p);}
  }
  public static void SpiSet(uint action,byte[] bytes) { IntPtr p=Marshal.AllocHGlobal(bytes.Length); try {Marshal.Copy(bytes,0,p,bytes.Length); Check(SystemParametersInfo(action,(uint)bytes.Length,p,3));} finally {Marshal.FreeHGlobal(p);} }
  public static T Decode<T>(byte[] bytes) { IntPtr p=Marshal.AllocHGlobal(bytes.Length); try {Marshal.Copy(bytes,0,p,bytes.Length); return (T)Marshal.PtrToStructure(p,typeof(T));} finally {Marshal.FreeHGlobal(p);} }
  public static byte[] Encode<T>(T value) { int size=Marshal.SizeOf(typeof(T)); IntPtr p=Marshal.AllocHGlobal(size); try {Marshal.StructureToPtr(value,p,false); byte[] b=new byte[size]; Marshal.Copy(p,b,0,size); return b;} finally {Marshal.FreeHGlobal(p);} }
  public static void Check(bool ok) { if(!ok) throw new Win32Exception(Marshal.GetLastWin32Error()); }
  public static void Broadcast() { IntPtr r; SendMessageTimeout(new IntPtr(0xffff),0x1a,IntPtr.Zero,"ImmersiveColorSet",2,2000,out r); SendMessageTimeout(new IntPtr(0xffff),0x1a,IntPtr.Zero,"Environment",2,2000,out r); }
 }
}

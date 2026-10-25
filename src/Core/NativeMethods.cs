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
   Check(IsWow64Process2(new IntPtr(-1),out processMachine,out nativeMachine),"프로세스 아키텍처 확인(IsWow64Process2)");
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
  [DllImport("kernel32.dll")] public static extern bool SetConsoleOutputCP(uint codePage);
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
    Check(SystemParametersInfo(action,parameter,p,0),"시스템 설정 읽기(SPI action=0x"+action.ToString("X")+")");
    Marshal.Copy(p,bytes,0,size);
    return bytes;
   } finally {Marshal.FreeHGlobal(p);}
  }
  public static void SpiSet(uint action,byte[] bytes) { IntPtr p=Marshal.AllocHGlobal(bytes.Length); try {Marshal.Copy(bytes,0,p,bytes.Length); Check(SystemParametersInfo(action,(uint)bytes.Length,p,3),(action==0x2a?"글꼴 설정(SPI_SETNONCLIENTMETRICS)":"아이콘 글꼴 설정(SPI_SETICONTITLELOGFONT)")+" action=0x"+action.ToString("X4"));} finally {Marshal.FreeHGlobal(p);} }
  public static T Decode<T>(byte[] bytes) { IntPtr p=Marshal.AllocHGlobal(bytes.Length); try {Marshal.Copy(bytes,0,p,bytes.Length); return (T)Marshal.PtrToStructure(p,typeof(T));} finally {Marshal.FreeHGlobal(p);} }
  public static byte[] Encode<T>(T value) { int size=Marshal.SizeOf(typeof(T)); IntPtr p=Marshal.AllocHGlobal(size); try {Marshal.StructureToPtr(value,p,false); byte[] b=new byte[size]; Marshal.Copy(p,b,0,size); return b;} finally {Marshal.FreeHGlobal(p);} }
  public static void Check(bool ok){Check(ok,"Win32 API");}
  public static void Check(bool ok,string what) {
   if(ok)return;
   int code=Marshal.GetLastWin32Error();
   string message=what+" 실패: "+(code==0?"Win32 API가 오류 코드 없이 실패를 반환했습니다":new Win32Exception(code).Message);
   message+=" (코드 0x"+code.ToString("X8")+", Marshal.GetLastWin32Error()="+code+")";
   Log.Write(message);throw new Win32Exception(code,message);
  }
  public static void ReloadCursors() {
   for(int attempt=0;attempt<2;attempt++) {
    if(SystemParametersInfo(0x57,0,IntPtr.Zero,0))return;
    int code=Marshal.GetLastWin32Error();
    Log.Write("경고: 커서 다시 읽기(SPI_SETCURSORS, action=0x0057) 실패 (코드 0x"+code.ToString("X8")+", Marshal.GetLastWin32Error()="+code+"). "+(attempt==0?"fWinIni=0으로 다시 시도합니다.":"레지스트리 설정은 보존되며 다음 로그인에 반영됩니다."));
   }
  }
  public static void Broadcast() { IntPtr r; SendMessageTimeout(new IntPtr(0xffff),0x1a,IntPtr.Zero,"ImmersiveColorSet",2,2000,out r); SendMessageTimeout(new IntPtr(0xffff),0x1a,IntPtr.Zero,"Environment",2,2000,out r); }
  [DllImport("msi.dll",CharSet=CharSet.Unicode)] static extern uint MsiOpenDatabase(string path,IntPtr persist,out uint database);
  [DllImport("msi.dll",CharSet=CharSet.Unicode)] static extern uint MsiDatabaseOpenView(uint database,string query,out uint view);
  [DllImport("msi.dll")] static extern uint MsiViewExecute(uint view,uint record);
  [DllImport("msi.dll")] static extern uint MsiViewFetch(uint view,out uint record);
  [DllImport("msi.dll",CharSet=CharSet.Unicode)] static extern uint MsiRecordGetString(uint record,uint field,StringBuilder value,ref uint length);
  [DllImport("msi.dll")] static extern uint MsiCloseHandle(uint handle);
  static void MsiCheck(uint result){if(result!=0)throw new InvalidOperationException("Open-Shell MSI 원본 정보 읽기 실패: "+result);}
  public static string MsiProductCode(string path) {
   uint database=0,view=0,record=0;
   try {
    MsiCheck(MsiOpenDatabase(path,IntPtr.Zero,out database));
    MsiCheck(MsiDatabaseOpenView(database,"SELECT `Value` FROM `Property` WHERE `Property`='ProductCode'",out view));
    MsiCheck(MsiViewExecute(view,0));MsiCheck(MsiViewFetch(view,out record));
    StringBuilder value=new StringBuilder(39);uint length=(uint)value.Capacity;MsiCheck(MsiRecordGetString(record,1,value,ref length));
    Guid code;if(!Guid.TryParse(value.ToString(),out code))throw new InvalidOperationException("Open-Shell MSI ProductCode가 올바르지 않습니다.");
    return code.ToString("B");
   }finally{if(record!=0)MsiCloseHandle(record);if(view!=0)MsiCloseHandle(view);if(database!=0)MsiCloseHandle(database);}
  }
  [DllImport("advapi32.dll",SetLastError=true)] static extern bool OpenProcessToken(IntPtr process,uint access,out IntPtr token);
  [DllImport("advapi32.dll",SetLastError=true)] static extern bool GetTokenInformation(IntPtr token,int information,out int value,int size,out int returned);
  [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr handle);
  public static bool ProcessElevated(IntPtr process) {
   IntPtr token;Check(OpenProcessToken(process,8,out token),"프로세스 토큰 열기(OpenProcessToken)");
   try{int value,returned;Check(GetTokenInformation(token,20,out value,4,out returned),"프로세스 권한 확인(GetTokenInformation)");return value!=0;}
   finally{CloseHandle(token);}
  }
 }
}

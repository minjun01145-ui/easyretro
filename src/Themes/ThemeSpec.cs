using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.Win32;
namespace EasyRetro {
 internal sealed class Setting {
  public string Key,Name; public object Value; public RegistryValueKind Kind;
  public Setting(string key,string name,object value) {Key=key;Name=name;Value=value;Kind=value is int?RegistryValueKind.DWord:value is byte[]?RegistryValueKind.Binary:RegistryValueKind.String;}
  public string Describe() {return "HKCU\\"+Key+" ["+(Name.Length==0?"(기본값)":Name)+"] "+Kind+" = "+(Value is byte[]?BitConverter.ToString((byte[])Value):Convert.ToString(Value));}
 }
 internal sealed class ThemeSpec {
  public string Id; public bool XP; public string Font; public Color Accent,Face;
  public static ThemeSpec Get(string id) {if(id!="98"&&id!="xp")throw new ArgumentException("테마는 98 또는 xp여야 합니다.");bool xp=id=="xp";return new ThemeSpec{Id=id,XP=xp,Font=xp?"Tahoma":"Microsoft Sans Serif",Accent=xp?Color.FromArgb(0,84,227):Color.Navy,Face=xp?Color.FromArgb(236,233,216):Color.Silver};}
  public static readonly int[] ColorIndexes={0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,26,27,28,29,30};
  public static readonly string[] ColorNames={"Scrollbar","Background","ActiveTitle","InactiveTitle","Menu","Window","WindowFrame","MenuText","WindowText","TitleText","ActiveBorder","InactiveBorder","AppWorkspace","Hilight","HilightText","ButtonFace","ButtonShadow","GrayText","ButtonText","InactiveTitleText","ButtonHilight","ButtonDkShadow","ButtonLight","InfoText","InfoWindow","HotTrackingColor","GradientActiveTitle","GradientInactiveTitle","MenuHilight","MenuBar"};
  public Color[] Colors() {Color highlight=XP?Color.FromArgb(49,106,197):Color.Navy;return new[]{Face,XP?Color.FromArgb(70,142,219):Color.Teal,Accent,XP?Color.FromArgb(122,150,223):Color.Gray,Face,Color.White,Color.Black,Color.Black,Color.Black,Color.White,Face,Face,Color.Gray,highlight,Color.White,Face,Color.Gray,Color.Gray,Color.Black,XP?Color.FromArgb(216,228,248):Color.Silver,Color.White,Color.FromArgb(64,64,64),XP?Color.FromArgb(241,239,226):Color.Silver,Color.Black,Color.FromArgb(255,255,225),highlight,XP?Color.FromArgb(61,149,255):Color.FromArgb(16,132,208),XP?Color.FromArgb(157,185,235):Color.Silver,highlight,Face};}
  public static int Bgr(Color c) {return c.R|(c.G<<8)|(c.B<<16);}
  public static string Rgb(Color c) {return c.R+" "+c.G+" "+c.B;}
 }
}

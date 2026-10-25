using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;
namespace EasyRetro {
 internal static class AssetGenerator {
  static readonly double[] durations={1.35,1.05,0.36,0.70,0.55,0.65};
  static readonly double[][] melodies={new[]{392.0,523.25,659.25},new[]{659.25,523.25,392.0},new[]{880.0,880.0,880.0},new[]{220.0,207.65,196.0},new[]{659.25,783.99,1046.50},new[]{523.25,392.0,261.63}};
  public static void Generate(string folder) {Directory.CreateDirectory(folder);Size size=Screen.PrimaryScreen.Bounds.Size;using(Bitmap b=Wallpaper(size.Width,size.Height))b.Save(Path.Combine(folder,"wallpaper-xp.png"),ImageFormat.Png);foreach(string id in new[]{"98","xp"}) {ThemeSpec t=ThemeSpec.Get(id);using(Bitmap b=Button(t))b.Save(Path.Combine(folder,"start-"+id+".png"),ImageFormat.Png);using(Bitmap b=Texture(t))b.Save(Path.Combine(folder,"taskbar-"+id+".png"),ImageFormat.Png);using(Bitmap b=Preview(t))b.Save(Path.Combine(folder,"preview-"+id+".png"),ImageFormat.Png);}string[] names={"logon","logoff","ding","error","notify","recycle"};for(int i=0;i<names.Length;i++)Wave(Path.Combine(folder,names[i]+".wav"),i);}
  public static Bitmap Wallpaper(int width,int height) {
   Bitmap b=new Bitmap(width,height);using(Graphics g=Graphics.FromImage(b)) {g.SmoothingMode=SmoothingMode.AntiAlias;g.ScaleTransform(width/1600f,height/900f);using(LinearGradientBrush sky=new LinearGradientBrush(new Rectangle(0,0,1600,900),Color.FromArgb(33,108,199),Color.FromArgb(190,227,247),90))g.FillRectangle(sky,0,0,1600,900);
    // Soft, original clouds; no source photograph or Microsoft artwork.
    for(int c=0;c<4;c++) {
     float x=90+c*420,y=95+(c%3)*67;
     for(int layer=4;layer>=0;layer--)using(GraphicsPath cloud=new GraphicsPath(FillMode.Winding)) {
      float spread=layer*3;
      cloud.AddEllipse(x-spread,y+22-spread,210+spread*2,35+spread*2);
      cloud.AddEllipse(x+35-spread,y+5-spread,92+spread*2,55+spread*2);
      cloud.AddEllipse(x+91-spread,y-spread,76+spread*2,66+spread*2);
      cloud.AddEllipse(x+145-spread,y+16-spread,61+spread*2,44+spread*2);
      using(SolidBrush white=new SolidBrush(Color.FromArgb(20,255,255,255)))g.FillPath(white,cloud);
     }
    }
    using(GraphicsPath distant=new GraphicsPath()) {distant.AddBezier(-100,630,370,390,970,620,1700,480);distant.AddLine(1700,480,1700,1000);distant.AddLine(1700,1000,-100,1000);distant.CloseFigure();using(LinearGradientBrush green=new LinearGradientBrush(new Rectangle(0,460,1600,450),Color.FromArgb(136,187,56),Color.FromArgb(38,111,29),90))g.FillPath(green,distant);}
    using(GraphicsPath hill=new GraphicsPath()) {hill.AddBezier(-100,690,470,370,820,520,1700,760);hill.AddLine(1700,760,1700,1000);hill.AddLine(1700,1000,-100,1000);hill.CloseFigure();using(LinearGradientBrush green=new LinearGradientBrush(new Rectangle(0,450,1600,500),Color.FromArgb(129,191,48),Color.FromArgb(33,108,32),105))g.FillPath(green,hill);}
   }return b;
  }
  public static Bitmap Button(ThemeSpec t) {
   // Open-Shell StartButton.cpp L587-L589: three equal vertical states: normal, hot, pressed.
   Bitmap b=new Bitmap(108,96,PixelFormat.Format32bppArgb);using(Graphics g=Graphics.FromImage(b)) {g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;for(int s=0;s<3;s++) {Rectangle r=new Rectangle(0,s*32,108,32);Color top=t.XP?(s==1?Color.FromArgb(127,200,87):Color.FromArgb(94,174,63)):(s==1?Color.FromArgb(220,220,220):Color.Silver);Color bottom=t.XP?Color.FromArgb(35,111,34):Color.FromArgb(180,180,180);if(s==2){top=bottom;bottom=t.XP?Color.FromArgb(60,135,45):Color.Silver;}using(LinearGradientBrush fill=new LinearGradientBrush(r,top,bottom,90))g.FillRectangle(fill,r);ControlPaint.DrawBorder3D(g,r,s==2?Border3DStyle.Sunken:Border3DStyle.Raised);int y=s*32+(s==2?1:0);using(Pen p=new Pen(t.XP?Color.White:Color.Navy,2)) {g.DrawRectangle(p,10,y+9,13,12);g.DrawLine(p,10,y+13,23,y+13);g.DrawLine(p,14,y+21,19,y+21);}using(Font f=new Font("Tahoma",t.XP?13:11,FontStyle.Bold))using(SolidBrush text=new SolidBrush(t.XP?Color.White:Color.Black))g.DrawString(t.XP?"start":"Start",f,text,32,y+5);}}return b;
  }
  public static Bitmap Texture(ThemeSpec t) {Bitmap b=new Bitmap(128,32);using(Graphics g=Graphics.FromImage(b)) {using(LinearGradientBrush fill=new LinearGradientBrush(new Rectangle(0,0,128,32),t.XP?Color.FromArgb(70,148,255):Color.FromArgb(215,215,215),t.XP?Color.FromArgb(20,73,183):Color.Silver,90))g.FillRectangle(fill,0,0,128,32);using(Pen p=new Pen(t.XP?Color.FromArgb(137,190,255):Color.White))g.DrawLine(p,0,0,128,0);using(Pen p=new Pen(t.XP?Color.FromArgb(15,54,143):Color.Gray))g.DrawLine(p,0,31,128,31);}return b;}
  public static Bitmap Preview(ThemeSpec t) {Bitmap b=new Bitmap(640,380);using(Graphics g=Graphics.FromImage(b)) {if(t.XP)using(Bitmap sky=Wallpaper(640,380))g.DrawImageUnscaled(sky,0,0);else g.Clear(Color.Teal);
    using(Font small=new Font("Tahoma",10))using(Font title=new Font("Tahoma",11,FontStyle.Bold)) {
     // A fictional sample window, not a promise of bitmap-skinned Windows 11 frames.
     Rectangle window=new Rectangle(150,72,390,228);using(SolidBrush face=new SolidBrush(t.Face))g.FillRectangle(face,window);ControlPaint.DrawBorder3D(g,window,Border3DStyle.Raised);using(SolidBrush accent=new SolidBrush(t.Accent))g.FillRectangle(accent,154,76,382,28);g.DrawString("EasyRetro · "+(t.XP?"XP":"98"),title,Brushes.White,163,80);g.DrawString("×",title,Brushes.White,513,79);g.FillRectangle(Brushes.White,166,118,358,137);g.DrawString("A familiar feeling. A fresh start.",small,Brushes.Black,182,144);g.DrawString("Original artwork • Windows 11 inside",small,Brushes.DimGray,182,178);g.DrawString("Ready",small,Brushes.Black,164,272);
     using(Bitmap tex=Texture(t))g.DrawImage(tex,new Rectangle(0,348,640,32));using(Bitmap btn=Button(t))g.DrawImage(btn,new Rectangle(0,348,108,32),new Rectangle(0,0,108,32),GraphicsUnit.Pixel);g.DrawString("EasyRetro",small,t.XP?Brushes.White:Brushes.Black,126,356);g.DrawString("12:00",small,t.XP?Brushes.White:Brushes.Black,590,356);
     for(int i=0;i<2;i++) {int y=28+i*83;g.FillRectangle(Brushes.LightGray,34,y,27,23);g.FillRectangle(Brushes.Navy,37,y+3,21,14);g.DrawString(i==0?"Computer":"Recycle Bin",small,Brushes.White,15,y+30);}
    }
   }return b;}
  static void Wave(string path,int type) {
   const int rate=44100;
   double seconds=durations[type],noteLength=seconds/3;
   double[] notes=melodies[type];
   int count=(int)(rate*seconds);
   using(BinaryWriter w=new BinaryWriter(File.Create(path))) {
    w.Write(Encoding.ASCII.GetBytes("RIFF"));w.Write(36+count*2);
    w.Write(Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);
    w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);
    w.Write((short)2);w.Write((short)16);
    w.Write(Encoding.ASCII.GetBytes("data"));w.Write(count*2);
    for(int i=0;i<count;i++) {
     double t=i/(double)rate;
     int note=Math.Min(2,(int)(t/noteLength));
     double local=t-note*noteLength;
     double envelope=Math.Min(1,local/0.012)*Math.Exp(-local*7)*Math.Min(1,(noteLength-local)/0.025);
     double sample=(Math.Sin(2*Math.PI*notes[note]*local)+0.23*Math.Sin(4*Math.PI*notes[note]*local))*envelope;
     w.Write((short)(sample*11000));
    }
   }
  }
 }
}

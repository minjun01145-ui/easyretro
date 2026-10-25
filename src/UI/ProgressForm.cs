using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace EasyRetro {
 internal sealed class ProgressForm:Form {
  readonly TextBox details=new TextBox();readonly string action,theme;bool running=true;public int Result=1;
  public ProgressForm(string action,string theme){this.action=action;this.theme=theme;Text="EasyRetro · 적용 및 복구";Font=new Font("Malgun Gothic",10);AutoScaleDimensions=new SizeF(96,96);ClientSize=new Size(650,370);StartPosition=FormStartPosition.CenterScreen;AutoScaleMode=AutoScaleMode.Dpi;Label label=new Label{Text="설정을 안전하게 변경하고 있습니다. 잠시 기다려 주세요.",Dock=DockStyle.Top,Height=54,Padding=new Padding(18)};ProgressBar bar=new ProgressBar{Dock=DockStyle.Bottom,Height=20,Style=ProgressBarStyle.Marquee};details.Multiline=true;details.ReadOnly=true;details.ScrollBars=ScrollBars.Vertical;details.Dock=DockStyle.Fill;details.BackColor=Color.White;Controls.Add(details);Controls.Add(label);Controls.Add(bar);FormClosing+=delegate(object s,FormClosingEventArgs e){if(running)e.Cancel=true;};Shown+=Start;}
  void Start(object sender,EventArgs args){
   BackgroundWorker worker=new BackgroundWorker();
   string completionNotice="";
   worker.DoWork+=delegate{StateStore store=StateStore.Default();Log.Start(store);Engine engine=new Engine(store,delegate(string text){if(!IsDisposed&&IsHandleCreated)BeginInvoke((Action)delegate{if(!details.IsDisposed)details.AppendText(text+Environment.NewLine);});});if(action=="--apply")engine.Apply(theme);else{engine.Restore();if(action=="--uninstall")SelfInstall.Remove();}completionNotice=engine.CompletionNotice;};
   worker.RunWorkerCompleted+=delegate(object s,RunWorkerCompletedEventArgs e){running=false;worker.Dispose();if(e.Error!=null){MessageBox.Show(this,Log.Failure(e.Error),"EasyRetro · 오류",MessageBoxButtons.OK,MessageBoxIcon.Error);Result=1;}else{MessageBox.Show(this,"완료했습니다."+(string.IsNullOrEmpty(completionNotice)?"":Environment.NewLine+completionNotice),"EasyRetro",MessageBoxButtons.OK,MessageBoxIcon.Information);Result=0;}Close();};
   worker.RunWorkerAsync();
  }
 }
}

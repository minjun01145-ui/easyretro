using System;
using System.Collections.Generic;
using System.IO;
namespace EasyRetro {
 internal sealed class Engine {
  readonly StateStore store;readonly Action<string> progress;
  readonly List<IModule> modules=new List<IModule>{new WallpaperModule(),new SysColorsModule(),new DwmAccentModule(),new FontsModule(),new SoundsModule(),new CursorsModule(),new DesktopIconsModule(),new ExplorerPatcherModule(),new OpenShellModule(),new ShellModule()};
  public Engine(StateStore store,Action<string> progress){this.store=store;this.progress=progress;}
  void Report(string s){Log.Write(s);if(progress!=null)progress(s);}
  ApplyContext Context(AppState state){return new ApplyContext{Store=store,State=state};}
  public void Plan(string theme,bool restore){ThemeSpec t=ThemeSpec.Get(theme??"98");ApplyContext c=Context(store.Load());Report("DRY RUN: 실제 시스템 변경 없음 / Windows build "+NativeMethods.WindowsBuild());Report(restore?"원본 스냅샷 역순 복구; 값 kind/data 및 부재 복원; SPI 원본 재적용":"다운로드/해시 검증 → 최초 원본 스냅샷 1회 (전환 시 보존) → 트랜잭션 적용; 실패 시 현재 실행 이전 상태 복구");if(restore){for(int i=modules.Count-1;i>=0;i--){Report("["+modules[i].Id+"] Restore (멱등)");foreach(string s in modules[i].Plan(t,c))Report("  원본 복구 대상: "+s);}Report("EasyRetro가 설치한 Open-Shell만 저장된 ProductCode로 msiexec /x /qn /norestart; EP만 검증된 ep_setup /uninstall_silent (upstream 완료창 가능)");Report("Shell: 일반 권한 탐색기 재시작; state.json / backup / assets 삭제; logs 및 매니저 유지");}else{foreach(IModule m in modules){Report("["+m.Id+"] Backup / Apply");foreach(string s in m.Plan(t,c))Report("  "+s);}Report("[SelfInstall] %LOCALAPPDATA%\\Programs\\EasyRetro\\EasyRetro.exe / 시작메뉴 EasyRetro 매니저 / HKCU Uninstall 등록");}}
  public void Status(){AppState s=store.Load();Report(s==null?"상태: 적용된 테마 없음":"상태: "+s.appliedTheme+" / "+s.operation+" / 최초 스냅샷 "+s.snapshotCreatedUtc);Report("state: "+store.PathFor("state.json"));Report("지원 OS: Windows 11 x64 build "+NativeMethods.WindowsBuild());if(s!=null)foreach(KeyValuePair<string,ComponentState> pair in s.components)Report(pair.Key+": "+pair.Value.version+" installedByUs="+pair.Value.installedByUs);Plan(s==null?"98":s.appliedTheme??"98",false);Plan(s==null?"xp":s.appliedTheme??"xp",true);}
  public void Apply(string id){
   NativeMethods.WindowsBuild();ThemeSpec theme=ThemeSpec.Get(id);AppState previous=store.Load();if(previous!=null && previous.operation!="applied")throw new InvalidOperationException("중단된 작업이 있습니다. 먼저 원래대로 복구하세요.");bool first=previous==null;
   // Complete the network step before touching desktop settings or installing dependencies.
   foreach(Dependency d in new[]{Dependencies.EP,Dependencies.OS})if(Dependencies.Detect(d)==null)Downloader.Fetch(d,store.PathFor("cache"));
   AppState state=first?new AppState{snapshotCreatedUtc=DateTime.UtcNow.ToString("o")}:previous;string priorTheme=state.appliedTheme;
   string folder=store.PathFor(first?"backup":"rollback");if(Directory.Exists(folder))Directory.Delete(folder,true);Directory.CreateDirectory(folder);RegistryBackup before=new RegistryBackup(folder,false);ApplyContext c=Context(state);
   Report(first?"원본 스냅샷 생성":"현재 테마 트랜잭션 스냅샷 생성 (최초 백업 보존)");foreach(IModule m in modules)m.Backup(before,c);before.Save();
   if(!first)StateStore.WriteJson(Path.Combine(folder,"previous-state.json"),previous);
   state.operation="applying";state.completedModules.Clear();store.Save(state);
   List<IModule> attempted=new List<IModule>();
   try {
    Dependencies.Prepare(state,store);Report("오리지널 에셋 생성");AssetGenerator.Generate(c.Assets);
    foreach(IModule m in modules){Report(m.Id+" 적용");attempted.Add(m);state.completedModules.Add(m.Id);store.Save(state);m.Apply(theme,c);}
    SelfInstall.Install();state.appliedTheme=id;state.operation="applied";store.Save(state);if(!first)Directory.Delete(folder,true);Report("완료: "+id);
   }catch(Exception error){Report("적용 실패: "+error);List<Exception> errors=new List<Exception>();for(int i=attempted.Count-1;i>=0;i--){try{attempted[i].Restore(before,c);}catch(Exception e){errors.Add(e);Report("롤백 실패: "+e);}}
    if(first){
     foreach(Dependency d in new[]{Dependencies.OS,Dependencies.EP})try{Dependencies.Remove(d,state);}catch(Exception e){errors.Add(e);Report(e.ToString());}
     // Uninstallers can recreate user settings; the original trees remain authoritative.
     foreach(string module in new[]{"OpenShell","ExplorerPatcher"})try{before.Restore(module);}catch(Exception e){errors.Add(e);Report(e.ToString());}
    }
    if(attempted.Count>0)try{ShellRestart.Restart();}catch(Exception e){errors.Add(e);Report(e.ToString());}
    if(errors.Count==0){if(first)store.CleanTheme();else{state=StateStore.ReadJson<AppState>(Path.Combine(folder,"previous-state.json"));store.Save(state);Directory.Delete(folder,true);}}else {state.operation="recovery-required";state.appliedTheme=priorTheme;store.Save(state);throw new AggregateException("적용 및 롤백 실패. 백업을 보존했습니다. --restore로 복구하세요.",new[]{error,new AggregateException(errors)});}throw;
   }
  }
  public void Restore(){AppState state=store.Load();if(state==null){Report("적용된 테마가 없습니다. 복구할 변경 없음.");return;}RegistryBackup original=new RegistryBackup(store.PathFor("backup"),true);ApplyContext c=Context(state);state.operation="restoring";store.Save(state);List<Exception> errors=new List<Exception>();for(int i=modules.Count-1;i>=0;i--){try{Report(modules[i].Id+" 원본 복구");modules[i].Restore(original,c);}catch(Exception e){errors.Add(e);Report(e.ToString());}}
   foreach(Dependency d in new[]{Dependencies.OS,Dependencies.EP})try{Dependencies.Remove(d,state);}catch(Exception e){errors.Add(e);Report(e.ToString());}
   // Installers may change their user settings; restore the owned setting trees once more after removal.
   foreach(string module in new[]{"OpenShell","ExplorerPatcher"})try{original.Restore(module);}catch(Exception e){errors.Add(e);Report(e.ToString());}
   try{ShellRestart.Restart();}catch(Exception e){errors.Add(e);Report(e.ToString());}if(errors.Count>0)throw new AggregateException("일부 복구 실패: 백업과 상태 보존. 다시 복구할 수 있습니다.",errors);store.CleanTheme();string rollback=store.PathFor("rollback");if(Directory.Exists(rollback))Directory.Delete(rollback,true);Report("원래 Windows 설정으로 복구 완료");
  }
 }
}

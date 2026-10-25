using System;
using System.Collections.Generic;
using System.IO;
namespace EasyRetro {
 internal sealed class Engine {
  readonly StateStore store;readonly Action<string> progress;
  readonly List<IModule> modules=new List<IModule>{new WallpaperModule(),new SysColorsModule(),new DwmAccentModule(),new FontsModule(),new SoundsModule(),new CursorsModule(),new DesktopIconsModule(),new ExplorerPatcherModule(),new OpenShellModule(),new ShellModule()};
  public Engine(StateStore store,Action<string> progress){this.store=store;this.progress=progress;}
  public string CompletionNotice {get;private set;}
  void Report(string text){Log.Write(text);if(progress!=null)progress(text);}
  ApplyContext Context(AppState state){return new ApplyContext{Store=store,State=state};}
  public void Plan(string theme,bool restore) {
   ThemeSpec t=ThemeSpec.Get(theme??"98");ApplyContext context=Context(store.Load());
   Report("DRY RUN: 실제 시스템 변경 없음 / Windows build "+NativeMethods.WindowsBuild());
   Report(restore?"원본 스냅샷 역순 복구; 값 kind/data 및 부재 복원; SPI 원본 재적용":"다운로드/해시 검증 → 최초 원본 스냅샷 1회 (전환 시 보존) → 트랜잭션 적용; 실패 시 현재 실행 이전 상태 복구");
   if(restore) {
    for(int i=modules.Count-1;i>=0;i--){Report("["+modules[i].Id+"] Restore (멱등)");foreach(string text in modules[i].Plan(t,context))Report("  원본 복구 대상: "+text);}
    Report("EasyRetro가 설치한 Open-Shell만 저장된 ProductCode로 msiexec /x /qn /norestart; EP만 검증된 ep_setup /uninstall_silent (upstream 완료창 가능)");
    Report("제거 후 installer-side HKCU/HKLM 및 EP/Open-Shell 설정 원본 재복원 → 현재 세션의 모든 Explorer 종료 → 자동 재시작/LIMITED 일반 권한 쉘 확인");
    Report("state.json / backup / rollback / assets 삭제; logs 및 매니저 유지");
   }else {
    if(t.XP)foreach(string asset in XpAssets.Plan(context.Assets))Report("[XP 에셋] "+asset);
    foreach(IModule module in modules){Report("["+module.Id+"] Backup / Apply");foreach(string text in module.Plan(t,context))Report("  "+text);}
    Report("설치 → Open-Shell 종료/NOSTART=1 → 설정 기록/첫 실행 설정창 방지 → 모든 현재 세션 Explorer 종료 → 자동 재시작/LIMITED 일반 권한 쉘 확인");
    Report("설치 전 MSI ProductCode/부분 설치 저널; 중단 시 매니저에서 최초 원본 복구 가능; 자기설치 실패도 롤백");
   }
  }
  public void Status() {
   AppState state=store.Load();Report(state==null?"상태: 적용된 테마 없음":"상태: "+state.appliedTheme+" / "+state.operation+" / 최초 스냅샷 "+state.snapshotCreatedUtc);
   Report("state: "+store.PathFor("state.json"));Report("지원 OS: Windows 11 x64 build "+NativeMethods.WindowsBuild());
   if(state!=null)foreach(KeyValuePair<string,ComponentState> pair in state.components)Report(pair.Key+": "+pair.Value.version+" installedByUs="+pair.Value.installedByUs);
   Plan(state==null?"98":state.appliedTheme??"98",false);Plan(state==null?"xp":state.appliedTheme??"xp",true);
  }
  void Attempt(Action action,List<Exception> errors){try{action();}catch(Exception error){errors.Add(error);Report(error.ToString());}}
  static bool Captured(RegistryBackup backup,IModule module){return backup.HasModule(module.Id)||(module.Id=="Shell"&&File.Exists(Path.Combine(backup.Folder,"shell-user.txt")));}
  void RestoreInstallerSettings(RegistryBackup backup,List<Exception> errors,Predicate<string> attempted) {
   foreach(string name in new[]{"OpenShell","ExplorerPatcher","InstallerOS","InstallerEP"}) {
    string module=name,owner=name=="InstallerOS"?"OpenShell":name=="InstallerEP"?"ExplorerPatcher":name;
    if(backup.HasModule(module)&&attempted(owner))Attempt(delegate{backup.Restore(module);},errors);
   }
  }
  void RemoveRollbackFolder(string folder){try{if(Directory.Exists(folder))StateStore.DeleteDirectory(folder);}catch(IOException error){Report("완료된 트랜잭션 임시 백업 정리 보류: "+error.Message);}catch(UnauthorizedAccessException error){Report("완료된 트랜잭션 임시 백업 정리 보류: "+error.Message);}}
  public void Apply(string id) {
   NativeMethods.WindowsBuild();ThemeSpec theme=ThemeSpec.Get(id);AppState previous=store.Load();
   if(previous!=null&&previous.operation!="applied")throw new InvalidOperationException("중단된 작업이 있습니다. 먼저 원래대로 복구하세요.");
   bool first=previous==null;Dictionary<string,bool> present=new Dictionary<string,bool>();
   foreach(Dependency dependency in new[]{Dependencies.EP,Dependencies.OS}){bool exists=Dependencies.Detect(dependency)!=null;present[dependency.Id]=exists;if(!exists)Downloader.Fetch(dependency,store.PathFor("cache"));}
   using(PreparedXpAssets xp=theme.XP?XpAssets.Prepare(store.PathFor("cache"),Report):null) {
   AppState state=first?new AppState{snapshotCreatedUtc=DateTime.UtcNow.ToString("o")}:previous;string priorTheme=state.appliedTheme;
   string folder=store.PathFor(first?"backup":"rollback");if(Directory.Exists(folder))StateStore.DeleteDirectory(folder);Directory.CreateDirectory(folder);
   RegistryBackup before=new RegistryBackup(folder,false);ApplyContext context=Context(state);
   Report(first?"원본 스냅샷 생성":"현재 테마 트랜잭션 스냅샷 생성 (최초 백업 보존)");
   XpAssets.Backup(context.Assets,folder);
   foreach(IModule module in modules)module.Backup(before,context);Dependencies.Backup(before);ComponentFiles.Backup(folder);SelfInstall.Backup(before);
   if(!first)StateStore.WriteJson(Path.Combine(folder,"previous-state.json"),previous);
   before.Save();state.operation="applying";state.completedModules.Clear();store.Save(state);
   List<IModule> attempted=new List<IModule>();bool managerAttempted=false;string failedStep="에셋 게시";
   try {
    failedStep="구성요소 준비";Dependencies.Prepare(state,store);failedStep="에셋 게시";Report("에셋 게시 (XP 원본은 스냅샷 전에 다운로드/검증 완료)");AssetGenerator.Ensure(context.Assets);if(xp!=null)XpAssets.Publish(xp,context.Assets);
    foreach(IModule module in modules){failedStep=module.Id;Report(module.Id+" 적용");attempted.Add(module);state.completedModules.Add(module.Id);store.Save(state);module.Apply(theme,context);}
    failedStep="매니저 설치";managerAttempted=true;state.selfInstallPending=true;store.Save(state);SelfInstall.Install();
    state.selfInstallPending=false;state.appliedTheme=id;state.operation="applied";store.Save(state);
   }catch(Exception error) {
    Report("적용 실패: "+error);List<Exception> errors=new List<Exception>();
    Attempt(delegate{XpAssets.Rollback(context.Assets,folder);},errors);
    // Include the failing module: it may have mutated several values before throwing.
    for(int i=attempted.Count-1;i>=0;i--){IModule module=attempted[i];if(Captured(before,module))Attempt(delegate{module.Restore(before,context);},errors);}
    foreach(Dependency dependency in new[]{Dependencies.OS,Dependencies.EP})if(!present[dependency.Id]&&attempted.Exists(delegate(IModule m){return m.Id==(dependency==Dependencies.OS?"OpenShell":"ExplorerPatcher");})){Dependency item=dependency;Attempt(delegate{Dependencies.Remove(item,state,store);},errors);}
    RestoreInstallerSettings(before,errors,delegate(string name){return attempted.Exists(delegate(IModule m){return m.Id==name;});});
    if(managerAttempted)Attempt(delegate{SelfInstall.Rollback(before);state.selfInstallPending=false;store.Save(state);},errors);
    if(attempted.Count>0)Attempt(ShellRestart.Restart,errors);
    Attempt(delegate{ComponentFiles.Restore(folder,state);},errors);
    if(errors.Count==0) {
     if(first)store.CleanTheme();else{store.Save(StateStore.ReadJson<AppState>(Path.Combine(folder,"previous-state.json")));RemoveRollbackFolder(folder);}
    }else{state.operation="recovery-required";state.appliedTheme=priorTheme;store.Save(state);throw new AggregateException("적용 실패 단계: "+failedStep+". 롤백 중 오류가 발생했습니다. 백업을 보존했습니다. --verify로 남아 있는 변경을 확인하고 매니저의 원래대로 복구를 실행하세요.",new[]{error,new AggregateException(errors)});}
    throw new InvalidOperationException("적용 실패 단계: "+failedStep+". 시스템을 적용 이전 상태로 복구했습니다.",error);
   }
   // Cleanup is not part of apply: failure deleting a temp backup must never undo committed success.
   if(!first)RemoveRollbackFolder(folder);CompletionNotice=xp==null?"":xp.Notice;Report("완료: "+id);if(!string.IsNullOrEmpty(CompletionNotice))Report(CompletionNotice);
   }
  }
  public void Restore() {
   AppState state=store.Load();if(state==null){Report("적용된 테마가 없습니다. 복구할 변경 없음.");return;}
   if(state.operation=="cleaning"){store.CleanTheme();Report("완료된 복구의 임시 파일 정리 완료");return;}
   RegistryBackup original=new RegistryBackup(store.PathFor("backup"),true);ApplyContext context=Context(state);
   state.operation="restoring";store.Save(state);List<Exception> errors=new List<Exception>();
   Predicate<string> attempted=delegate(string name){return state.appliedTheme!=null||state.completedModules.Contains(name);};
   for(int i=modules.Count-1;i>=0;i--){IModule module=modules[i];if(!attempted(module.Id)||!Captured(original,module))continue;Report(module.Id+" 원본 복구");Attempt(delegate{module.Restore(original,context);},errors);}
   foreach(Dependency dependency in new[]{Dependencies.OS,Dependencies.EP})if(attempted(dependency==Dependencies.OS?"OpenShell":"ExplorerPatcher")){Dependency item=dependency;Attempt(delegate{Dependencies.Remove(item,state,store);},errors);}
   // Uninstallers can recreate settings; restore their captured keys AFTER removal.
   RestoreInstallerSettings(original,errors,attempted);
   if(state.selfInstallPending)Attempt(delegate{string path=store.PathFor("rollback");RegistryBackup manager=File.Exists(Path.Combine(path,"previous-state.json"))?new RegistryBackup(path,true):original;SelfInstall.Rollback(manager);state.selfInstallPending=false;store.Save(state);},errors);
   // Always last, including failed uninstall: replace any shell launched by EP itself.
   Attempt(ShellRestart.Restart,errors);
   Attempt(delegate{ComponentFiles.Restore(original.Folder,state);},errors);
   if(errors.Count>0)throw new AggregateException("일부 복구 실패: 백업과 상태를 보존했습니다. 다시 복구할 수 있습니다.",errors);
   store.CleanTheme();Report("원래 Windows 설정으로 복구 완료");
  }
 }
}

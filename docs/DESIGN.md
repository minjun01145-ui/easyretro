# EasyRetro 설계 (v0.1)

Windows 11 전용. 실행 → "Windows 98" / "Windows XP" 중 하나 클릭 → 끝. 이후 같은 프로그램(매니저)에서 테마 전환·원상복구.

## 1. 목표 / 비목표

| 목표 | 방법 |
| --- | --- |
| 원클릭 적용 | 단일 `EasyRetro.exe` (외부 런타임 불필요, .NET Framework 4.8 = Win11 기본 내장) |
| 소리·작업표시줄·창·시작버튼·배경·커서·글꼴 변경 | 모듈별 적용 (§4) |
| Win11 편의기능 유지 | 탐색기 탭(기본 Win11 탐색기 유지), 작업표시줄 그룹화(항상 결합), 시작메뉴 프로그램 검색(Open-Shell 검색창 + Shift+Win = 원래 Win11 시작) |
| 깔끔한 원상복구 | 최초 적용 직전 스냅샷 1회 → 복구 시 정확히 되돌리고, EasyRetro가 설치한 구성요소만 제거 |

비목표(v0.1): 비서명 msstyles(창 테두리 비트맵 스킨)·시스템 파일 패치·Windhawk. 창은 DWM 강조색(XP 파랑 / 98 남색 제목표시줄) + 클래식 시스템 색상 + 글꼴로 표현. 이후 버전에서 선택형 고급 모듈로 검토.

저작권: Microsoft 원본 그림·소리를 재배포하지 않는다. 배경화면·시작버튼·작업표시줄 텍스처·미리보기·XP풍 효과음은 **런타임에 코드로 생성한 오리지널**. 98 효과음은 Win11에 이미 있는 `C:\Windows\Media\{chimes,chord,ding,notify,recycle,tada}.wav`를 참조만 한다. 저장소에 바이너리 에셋 없음.

## 2. 구성요소 (자동, 무인 설치)

사용자는 아무것도 따로 설치하지 않는다. 적용 시 공식 GitHub 릴리스에서 **버전 고정 + SHA-256 검증** 후 무인 설치.

| 구성요소 | 역할 | 비고 |
| --- | --- | --- |
| ExplorerPatcher | Windows 10식 작업표시줄(텍스처 스킨 가능, 그룹화 지원) | 탐색기 명령바/리본 옵션은 건드리지 않음 → 탭 유지 |
| Open-Shell (StartMenu 기능만) | 98(Classic)/XP(Luna 2단) 시작메뉴, 커스텀 시작버튼 이미지, 검색창, 작업표시줄 텍스처/글자색 | `ADDLOCAL=StartMenu` 만. Classic Explorer 미설치(탭 보존) |

- 이미 사용자가 설치해 둔 경우: 설치/제거하지 않고 설정 키만 백업·복원(`ownedByEasyRetro=false`).
- 무인 설치 인자, 레지스트리 값 이름, 26100/26200 빌드 호환 설정은 각 프로젝트 공식 문서/소스 기준으로 확정해 `Dependencies.cs`에 주석으로 출처 기록.

## 3. 사용자 흐름

```mermaid
flowchart TD
  A[EasyRetro.exe 실행<br/>권한상승 없음] --> B{state.json 존재?}
  B -- 아니오 --> C[선택 화면: 98 카드 / XP 카드]
  B -- 예 --> D[매니저: 현재 테마 표시<br/>98로 변경 / XP로 변경 / 원래대로 복구]
  C -- 클릭 --> E[자기 자신을 runas로 재실행<br/>--apply 98 또는 xp : UAC 1회]
  D -- 클릭 --> E
  D -- 복구 --> R[runas --restore]
  E --> F[진행창: 다운로드/검증 → 스냅샷(최초 1회) → 모듈 적용 → 탐색기 재시작]
  F --> G[완료 메시지 + 자기설치<br/>시작메뉴 'EasyRetro 매니저' + 앱 및 기능 등록]
```

- GUI는 `asInvoker`로 뜬다(미리보기·상태 확인은 권한 불필요). 실제 작업만 상승된 자식 프로세스가 수행하고 GUI는 종료코드를 기다린 뒤 상태 갱신.
- 상승 시 HKCU 대상: 관리자 계정이면 동일 사용자 하이브. 표준 사용자+타 관리자 자격증명(OTS 상승)은 v0.1 미지원 → 감지 시 안내 후 중단(상승 프로세스의 사용자 SID ≠ 원 GUI 사용자 SID 비교, SID는 인자로 전달).
- 테마 간 전환 시 스냅샷은 다시 찍지 않는다(원본 Win11 상태 보존). 스냅샷은 "적용된 테마 없음" 상태에서만 생성.

## 4. 모듈

각 모듈: `Id`, `Plan(theme)`(드라이런 출력용 동작 목록), `Backup(store)`(만지는 값 캡처), `Apply(theme, ctx)`, `Restore(store, ctx)`. 복구는 멱등이며 일부 대상이 이미 사라져도 계속 진행.

| 모듈 | 98 | XP | 적용 방식 |
| --- | --- | --- | --- |
| Wallpaper | 단색 청록 (0,128,128), 배경그림 없음 | 생성한 "푸른 하늘+녹색 언덕" 오리지널 이미지(주 모니터 해상도) | `SPI_SETDESKWALLPAPER`, `Control Panel\Colors\Background`, `Control Panel\Desktop` WallpaperStyle |
| SysColors | 클래식 회색 팔레트(ButtonFace 192,192,192 / ActiveTitle 0,0,128 / GradientActiveTitle 16,132,208 등) | Luna풍 팔레트(ButtonFace 236,233,216 / ActiveTitle 0,84,227 / Hilight 49,106,197 등) | `SetSysColors`(즉시) + `HKCU\Control Panel\Colors`(영구) |
| DwmAccent | 제목표시줄 남색 | 제목표시줄 XP 파랑 | `HKCU\Software\Microsoft\Windows\DWM` AccentColor/ColorizationColor/ColorPrevalence=1, `...\Themes\Personalize` ColorPrevalence=1, EnableTransparency=0, Apps/SystemUsesLightTheme=1, `...\Explorer\Accent` AccentPalette/AccentColorMenu, 이후 `WM_SETTINGCHANGE "ImmersiveColorSet"` |
| Fonts | Microsoft Sans Serif | Tahoma | `SPI_GET/SETNONCLIENTMETRICS`, `SPI_SETICONTITLELOGFONT` + `SPIF_UPDATEINIFILE|SPIF_SENDCHANGE`. 원본 구조체 바이트를 base64로 백업 |
| Sounds | 스킴 `EasyRetro98`: Win11 내장 레거시 wav 매핑 | 스킴 `EasyRetroXP`: 생성한 오리지널 wav(로그온/로그오프/딩/오류/알림/휴지통) | `HKCU\AppEvents\Schemes\Apps\...\<event>\.Current`, `Schemes` 기본값, `Schemes\Names\<scheme>` |
| Cursors | Win11 내장 레거시 커서(`C:\Windows\Cursors`의 비-aero 세트, HKLM 커서 스킴 목록에서 확인) | Win11 내장 "Windows 표준" 계열 | `HKCU\Control Panel\Cursors` + `SPI_SETCURSORS` |
| DesktopIcons | 내 컴퓨터·휴지통 표시 | 동일 | `...\Explorer\HideDesktopIcons\NewStartPanel` CLSID 값 |
| ExplorerPatcher | Win10식 작업표시줄, 좌측정렬, 항상 결합(그룹화), 작은 아이콘 | 동일, 기본 크기 | EP 레지스트리 키 + `Explorer\Advanced` TaskbarGlomLevel=0 등. 파일탐색기/리본 관련 값 금지 |
| OpenShell | 스타일 Classic, 스킨 Classic Skin, 생성한 회색 "Start" 버튼, 회색 작업표시줄 텍스처 + 검정 글자, 검색창 표시 | 스타일 2단, 스킨 Windows XP Luna, 생성한 녹색 "start" 버튼, 파랑 그라데이션 작업표시줄 텍스처 + 흰 글자, 검색창 표시 | `HKCU\Software\OpenShell\StartMenu\Settings` 값 직접 기록(또는 `StartMenu.exe -xml`). Win 키 → Open-Shell, Shift+Win → Win11 원래 시작(검색 포함). 작업표시줄 스킨 활성화 값은 `SkinTaskbar`가 아니라 `CustomTaskbar` ([4.4.198 소스](https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/StartMenu/StartMenuDLL/SettingsUI.cpp#L4465)); Classic 스킨의 실제 파일명은 `Classic Skin.skin` ([설치 소스](https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/Src/Setup/Setup.wxs#L524)) |
| Shell | — | — | 마지막에 1회: `WM_SETTINGCHANGE` 브로드캐스트 + 탐색기 재시작 |

시작버튼/텍스처 그림: Microsoft 로고 사용 금지(4색 깃발 대신 일반적인 레트로 아이콘).

## 5. 상태·백업

`%LOCALAPPDATA%\EasyRetro\`
- `state.json`: `{ version, appliedTheme, snapshotCreatedUtc, components: { explorerPatcher: {installedByUs, version}, openShell: {installedByUs, productCode, version} }, completedModules: [...] }`
- `backup\values.json`: 모듈이 만지는 각 레지스트리 값 `{hive, key, name, existed, kind, data}`. 키 전체 소유 대상(OpenShell/EP 설정)은 `reg export` 파일 + `keyExisted`.
- `backup\nonclientmetrics.bin`, `backup\iconfont.bin`, 기타 SPI 원본값.
- `assets\` 생성된 그림/소리, `cache\` 다운로드 설치 파일, `logs\easyretro-YYYYMMDD.log`.

트랜잭션: 적용 중 실패 → 이번 실행에서 완료한 모듈을 역순 복구, 최초 적용이었다면 전체 복구 후 state 삭제. 실패 메시지는 로그 경로와 함께 표시.

## 6. 원상복구 (`--restore`)

1. 모듈 역순 Restore(값 존재했으면 원래 kind/data로, 없었으면 삭제). SPI 원본값 재적용.
2. `installedByUs=true` 구성요소만 무인 제거(Open-Shell: 저장한 MSI ProductCode로 `msiexec /x /qn`, EP: 공식 `/uninstall_silent`). 단, EP의 이 인자는 최초 확인만 생략하며 완료·재시작·오류 창은 표시될 수 있다 ([26100.8457.70.3 설치 소스](https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/ep_setup/ep_setup.c#L1674-L1689)).
3. 탐색기 재시작, 브로드캐스트.
4. `state.json`·백업 삭제(로그는 남김). 생성 에셋 삭제.

`--uninstall`(앱 및 기능에서 호출): restore → 시작메뉴 바로가기·Uninstall 키 제거 → 설치폴더 지연삭제.

## 7. 자기 설치

최초 적용 성공 시 `%LOCALAPPDATA%\Programs\EasyRetro\EasyRetro.exe`로 복사, 사용자 시작메뉴 `EasyRetro 매니저.lnk`, `HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\EasyRetro`(DisplayName, UninstallString=`"…\EasyRetro.exe" --uninstall`, DisplayIcon, Publisher, DisplayVersion, NoModify/NoRepair). 다운로드 폴더에서 실행했어도 이후엔 매니저 바로가기로 관리.

## 8. 탐색기 재시작 (상승 프로세스 주의)

상승 프로세스가 `explorer.exe`를 직접 띄우면 쉘이 상승 상태로 뜰 수 있다. 순서: 탐색기 정상 종료 요청(`Shell_TrayWnd`에 종료 메시지 → 실패 시 프로세스 종료) → `AutoRestartShell`에 의한 자동 재시작 대기(최대 8초) → 미재시작 시 일반 권한으로 실행(작업 스케줄러 `/RL LIMITED /IT` 일회성 작업 생성·실행·삭제).

## 9. CLI

| 인자 | 권한 | 동작 |
| --- | --- | --- |
| (없음) | 일반 | GUI |
| `--apply 98\|xp` | 상승(자동 runas) | 적용 + 진행창 |
| `--restore` | 상승 | 원상복구 |
| `--uninstall` | 상승 | 복구 + 앱 제거 |
| `--status` | 일반 | 현재 상태 출력 |
| `--dry-run --apply 98\|xp` / `--dry-run --restore` | 일반 | 변경 없이 계획 동작 목록 출력 |
| `--selftest` | 일반 | 에셋을 임시폴더에 생성, 의존성 URL 다운로드+SHA-256 검증, OS 빌드 확인. 시스템 변경 없음 |

Win11(빌드 ≥ 22000) 아니면 거부. 중복 실행 방지 뮤텍스.

## 10. 빌드·배포

- 언어: C# 5 (Win11 내장 `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe`로 빌드 가능해야 함 → 문자열 보간·`?.`·`nameof`·식 본문 멤버 금지). WinForms, `System.Web.Extensions`(JavaScriptSerializer), `System.Drawing`.
- `build.cmd` → `dist\EasyRetro.exe`. 매니페스트: `asInvoker`, PerMonitorV2 DPI, Win10+ supportedOS.
- GitHub Actions(`windows-latest`)가 태그 푸시 시 같은 `build.cmd`로 빌드해 릴리스에 `EasyRetro.exe` 첨부. 사용자는 Releases에서 exe 하나만 받는다.
- UI 문구 한국어.

## 11. 소스 구조

```
src/
  Program.cs                 진입점, 인자 파싱, 상승 재실행, 뮤텍스
  UI/MainForm.cs             선택 화면 + 매니저
  UI/ProgressForm.cs         상승 프로세스 진행창
  Core/Engine.cs             적용/복구 오케스트레이션, 트랜잭션
  Core/StateStore.cs         state.json, values.json
  Core/RegistryBackup.cs     값 단위 캡처/복원, 키 export/import
  Core/Downloader.cs         TLS1.2, 캐시, SHA-256
  Core/Dependencies.cs       고정 버전·URL·해시·무인 인자(출처 주석)
  Core/ShellRestart.cs
  Core/SelfInstall.cs        자기 설치, 바로가기, Uninstall 키, 지연 삭제
  Core/NativeMethods.cs
  Core/Log.cs
  Themes/ThemeSpec.cs        98/XP 정의(데이터만)
  Modules/*.cs               §4 모듈들
  Assets/AssetGenerator.cs   배경·시작버튼·텍스처·미리보기 그림, wav 합성
app.manifest
build.cmd
.github/workflows/release.yml
README.md (한국어: 다운로드→실행→선택, 복구, 설치되는 것, 크레딧/라이선스)
LICENSE (MIT)
```

## 12. 알려진 한계 (README에 명시)

- Win11 창 둥근 모서리·Mica 일부 앱(UWP/WinUI 설정 앱 등)은 테마 미적용.
- 창 테두리 비트맵 스킨(진짜 XP 창)은 v0.1 범위 밖.
- ExplorerPatcher는 일부 백신이 오탐할 수 있음. Windows 대형 업데이트 직후 EP 호환 버전이 필요할 수 있음.

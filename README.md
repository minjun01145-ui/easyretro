# EasyRetro

Windows 11을 Windows 98 / Windows XP 분위기로 바꾸는 작은 테마 도구입니다. 배경, 시작메뉴와 시작버튼, 작업표시줄, 시스템 색상, 글꼴, 커서, 효과음을 한 번에 설정하고 매니저에서 전환하거나 원래대로 복구합니다.

## 시작하기

1. 이 저장소의 **Releases**에서 **`EasyRetro.exe` 하나를 다운로드**합니다.
2. 실행하고 **Windows 98 적용** 또는 **Windows XP 적용**을 클릭합니다.
3. 관리자 권한 요청(UAC)을 승인하고 완료 안내를 기다립니다. 처음에는 인터넷 연결이 필요합니다.

별도의 .NET 설치는 필요하지 않습니다. Windows 11에 포함된 .NET Framework를 사용합니다. **Windows 11 x64 전용**이며 ARM64는 지원하지 않습니다. 일반 관리자 계정의 UAC 승격만 지원합니다. 표준 사용자 계정에서 다른 관리자 계정의 암호를 입력하는 방식은 사용자 설정을 잘못 바꾸지 않도록 거부합니다.

적용 전 작업을 저장하세요. 설정을 반영하려고 탐색기를 재시작하므로 열려 있던 탐색기 창이 닫힐 수 있습니다. 회사·업무용 PC보다 복구 가능한 개인 테스트 환경에서 먼저 사용하기를 권장합니다.

### 유지하는 Windows 11 기능

- 기본 파일 탐색기를 계속 사용하므로 **탭 기능 유지**. 레거시 리본/명령바 설정은 변경하지 않습니다.
- 작업표시줄은 **항상 결합(묶기)**, 왼쪽 정렬입니다.
- Open-Shell 시작메뉴의 **프로그램 검색창 사용 가능**.
- **Win**은 레트로 시작메뉴, **Shift+Win**은 원래 Windows 11 시작메뉴를 엽니다.

## 매니저 · 테마 전환 · 복구

첫 적용에 성공하면 시작메뉴에 **EasyRetro 매니저**가 생깁니다. 다운로드한 파일을 옮겨도 이 바로가기로 관리할 수 있습니다.

- 다른 분위기로 바꾸려면 매니저에서 98 / XP를 클릭합니다.
- **원래대로 복구**를 누르면 **최초 적용 직전** 설정을 되돌립니다. 전환할 때 원본 백업을 덮어쓰지 않습니다.
- 기존에 직접 설치한 ExplorerPatcher / Open-Shell은 제거하지 않고 사용자 설정을 복원합니다. EasyRetro가 새로 설치한 구성요소만 제거합니다.
- 복구 후 매니저는 남아 다시 테마를 선택할 수 있습니다.
- EasyRetro까지 지우려면 **설정 → 앱 → 설치된 앱 → EasyRetro → 제거**를 사용합니다. 먼저 테마를 복구한 뒤 바로가기와 앱 등록을 제거하고, 열려 있는 매니저가 모두 닫힌 후 설치 파일을 삭제합니다.

오류가 나면 실패 도중 일부만 변경된 모듈까지 포함해 역순으로 되돌립니다. 첫 적용 실패는 최초 상태로, 테마 전환 실패는 전환 직전 테마로 되돌리고 최초 Windows 백업은 보존합니다. 구성요소 설치 직전의 부분 설치 저널(MSI ProductCode 포함), 자기설치 변경도 복구 대상입니다. 전원 차단·프로세스 종료로 중단되면 매니저에서 **원래대로 복구**를 실행하세요. 복구 자체가 실패하거나 원본 백업 파일이 없으면 나머지 복구를 계속하고 백업과 상태를 삭제하지 않습니다. 복구 완료 후 파일 정리 도중 중단된 경우에는 다음 복구에서 정리만 다시 수행합니다. `%LOCALAPPDATA%\EasyRetro\logs`의 로그를 확인하세요. **적용 중에는 `state.json`이나 `backup` 폴더를 지우지 마세요.**

Windows 업데이트나 다른 테마 도구가 설정을 다시 바꾸면 결과가 달라질 수 있습니다. 복구는 EasyRetro가 보관한 최초 값 기준이며, 적용 후 다른 도구로 변경한 같은 설정도 원본으로 돌아갑니다.

## 자동 설치되는 구성요소 · 크레딧

공식 GitHub에서 아래 고정 버전을 다운로드하고 **SHA-256이 정확히 일치할 때만** 실행합니다. 설치 파일을 EasyRetro 실행 파일 안에 재배포하지 않습니다. 기존 설치가 있으면 재설치·업데이트·제거하지 않습니다.

| 구성요소 | 용도 · 설치 범위 | 버전 · 라이선스 |
| --- | --- | --- |
| [ExplorerPatcher](https://github.com/valinet/ExplorerPatcher) | Windows 10식 작업표시줄. Windows 11 24H2 이상은 재구현 작업표시줄 사용 | [26100.8457.70.3](https://github.com/valinet/ExplorerPatcher/releases/tag/26100.8457.70.3) · [GPL-2.0](https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/LICENSE) 및 배포물에 포함된 구성요소별 조건 |
| [Open-Shell](https://github.com/Open-Shell/Open-Shell-Menu) | 시작메뉴 기능만 (`ADDLOCAL=StartMenu`). Classic Explorer / Classic IE 미설치 | [4.4.198](https://github.com/Open-Shell/Open-Shell-Menu/releases/tag/v4.4.198) · [MIT](https://github.com/Open-Shell/Open-Shell-Menu/blob/v4.4.198/LICENSE), Ivo Beltchev 및 Open-Shell 팀 크레딧은 공식 배포물 참조 |

ExplorerPatcher의 공식 `/uninstall_silent`는 초기 확인을 생략하지만 **완료·재시작·오류 창이 추가로 뜰 수 있습니다**. 이 창은 upstream 동작이며 EasyRetro가 강제로 숨기거나 자동 클릭하지 않습니다.

Open-Shell은 `NOSTART=1`로 설치한 뒤 기존 시작메뉴 프로세스를 종료하고 설정을 기록합니다. 첫 실행 스타일 선택창을 표시하지 않으며 마지막 쉘 재시작 이후 일반 권한으로 시작합니다. EP 설치 프로그램이 먼저 탐색기를 재시작하더라도, EasyRetro가 **설치 → 설정 기록 → 최종 쉘 재시작** 순서로 다시 반영합니다.

ExplorerPatcher의 공식 릴리스는 **Smart App Control이 켜진 환경과 SentinelOne 설치 환경에서 쉘 시작/제거 문제가 생길 수 있음**을 경고합니다. 해당 환경에서는 사용하지 마세요. EasyRetro는 백신 예외를 추가하거나 보안 기능을 끄지 않습니다. [공식 릴리스 주의사항](https://github.com/valinet/ExplorerPatcher/releases/tag/26100.8457.70.3)을 먼저 확인하세요.

**파일 해시**

```text
ExplorerPatcher ep_setup.exe (26100.8457.70.3)
8146db4d3a87201fb80ad1d3712ba8f56883e9a0811758bd39f62d73a9f2c586
OpenShellSetup_4_4_198.exe
a4d2d4459de55b5e962ba2a14f7bb794170511649138173dfa72949837b48c3f
```

EasyRetro 자체 소스, 98 그림, 미리보기와 대체용 생성 에셋은 [MIT](LICENSE)입니다. **XP의 Bliss 사진·Luna 그림·Windows XP 효과음은 Microsoft 저작권 자산이며 MIT 대상이 아닙니다.** 공개 저장소와 배포 실행 파일에는 Microsoft 에셋을 넣지 않습니다. XP 적용 시 제3자 공개 보관소에서 원본을 내려받아 아래 고정 SHA-256·크기를 검사합니다. 공개 다운로드 가능 여부가 재배포 허가나 별도 사용권을 뜻하지는 않습니다. 해당 자산의 권리는 원저작권자에게 있으며 사용자는 적용되는 Microsoft 사용 조건을 확인해야 합니다.

### XP 원본 에셋 · 출처

| 자산 | 출처 페이지 / 직접 다운로드 | 두 번째 URL | SHA-256 | 크기 |
| --- | --- | --- | --- | --- |
| Bliss (XP 출시본 800×600, 업스케일 아님) | [보관 항목](https://archive.org/details/windows-xp-wallpapers_202310) / [bliss.jpg](https://archive.org/download/windows-xp-wallpapers_202310/bliss.jpg) | [IA 저장 노드](https://ia801609.us.archive.org/22/items/windows-xp-wallpapers_202310/bliss.jpg) | `eee00f376a6ff19c32e8670bf630bf8336f11247dd5ce74fb8cd91e5ca33327a` | 54,607 bytes |
| Windows XP Media 효과음 ZIP | [보관 항목](https://archive.org/details/windows-xp-sounds) / [Windows XP Sounds.zip](https://archive.org/download/windows-xp-sounds/Windows%20XP%20Sounds.zip) | [IA 저장 노드](https://ia800601.us.archive.org/22/items/windows-xp-sounds/Windows%20XP%20Sounds.zip) | `3259f9b70db0e1bf5b9b72ddf3736397adabf45569e02377e1882772ed3d8afd` | 8,118,271 bytes |
| 실제 XP 시작버튼의 3상태 캡처 | [Classic Shell 원 게시물](https://www.classicshell.net/forum/viewtopic.php?t=1152) / [첨부 550](https://www.classicshell.net/forum/download/file.php?id=550) | [동일 바이트 첨부 549](https://www.classicshell.net/forum/download/file.php?id=549) | `246c153ea4489a9dda5a28e2941ffbcd5fec80395963ea07ceaac567170b5dea` | 19,855 bytes |
| 원본 luna.msstyles의 파란 작업표시줄 비트맵 | [보관 항목](https://archive.org/details/luna_20240122) / [luna.msstyles](https://archive.org/download/luna_20240122/luna.msstyles) | [IA 저장 노드](https://ia800807.us.archive.org/12/items/luna_20240122/luna.msstyles) | `0dbeb9efb7165dc6d812b373b895b7cc84f86a674d0b13ee5e4e7d2ec4a6a852` | 4,190,352 bytes |

시작버튼 출처는 troy2000me의 실제 XP normal/hot/pressed 화면 캡처이며, Jcee/Gaurav의 투명 배경·ClearType 정리본입니다(새로 그린 모방 그림 아님). 원본 100×162 스트립의 각 100×54 프레임에서 위아래 12px 투명 여백만 잘라 **100×30 세 장을 세로로 쌓은 100×90 PNG**를 만듭니다. 앱이 픽셀을 확대하거나 다시 그리지 않으며 DPI 처리는 Open-Shell에 맡깁니다. 작업표시줄은 원본 `BLUE_TASKBARBACKGROUND_BMP` **50×28** 픽셀을 그대로 PNG로 변환합니다. `luna.msstyles`는 `LoadLibraryEx(LOAD_LIBRARY_AS_DATAFILE)`로 리소스만 읽고 실행·설치·시스템 테마 패치하지 않습니다. ZIP은 Windows 내장 .NET 압축 API로 필요한 WAV만 추출합니다. IA 미러는 같은 보관 항목의 별도 저장 노드이며, 시작버튼 미러는 같은 사이트의 별도 첨부이므로 독립 서비스 장애 대비는 아닙니다.

모든 원본은 **스냅샷/시스템 설정 변경 전에** 캐시 다운로드·해시 검증·변환을 마칩니다. 원본과 두 번째 URL이 모두 실패하면 **실패한 항목만** 기존 자체 생성 그림/효과음으로 대체하고 로그 및 완료창 한 줄에 대체 목록을 표시합니다. 다른 원본은 그대로 사용합니다. 다음 XP 적용 때 원본을 다시 시도하며, `--selftest`에서는 대체를 성공으로 인정하지 않습니다. 전환 실패 시 기존 에셋 파일도 되돌려 원래 스냅샷 경로가 다른 그림/소리를 참조하지 않게 합니다.

**XP AppEvents 매핑** (`Windows XP ` 접두사와 `.wav` 확장자는 아래 파일명에서 생략):

| 애플리케이션 / 이벤트 | 원본 파일 |
| --- | --- |
| `.Default / SystemStart` | Startup |
| `.Default / WindowsLogon`, `WindowsLogoff`, `SystemExit` | Logon Sound, Logoff Sound, Shutdown (각각) |
| `.Default / .Default` | Default |
| `.Default / SystemAsterisk`, `SystemQuestion` | Ding |
| `.Default / AppGPFault`, `SystemHand`, `SystemExclamation` | Error, Critical Stop, Exclamation (각각) |
| `.Default / MailBeep`, `Notification.Default` | Notify |
| `.Default / SystemNotification` | Balloon |
| `Explorer / EmptyRecycleBin` | Recycle |
| `.Default / Minimize` | Minimize |
| `.Default / RestoreUp`, `RestoreDown`, `Maximize` | Restore |
| `.Default / MenuCommand` | Menu Command |
| `.Default / DeviceConnect`, `DeviceDisconnect`, `DeviceFail` | Hardware Insert, Hardware Remove, Hardware Fail (각각) |
| `.Default / LowBatteryAlarm`, `CriticalBatteryAlarm`, `PrintComplete` | Battery Low, Battery Critical, Print complete (각각) |
| `Explorer / Navigating` | Start |

`.Current`와 `EasyRetroXP` 스킴을 모두 기록하고 최초 값을 백업합니다. 98로 전환하면 XP 전용 이벤트는 무음으로, 기존 여섯 98 이벤트는 Win11 내장 레거시 소리로 설정합니다. 98 효과음은 이미 설치된 `%WINDIR%\Media`의 `chimes`, `chord`, `ding`, `notify`, `recycle`, `tada.wav`를 참조합니다. 커서는 해당 PC의 HKLM 내장 스킴에서 실제 존재하는 비-aero Windows Standard 파일을 선택합니다.

## 알려진 한계

- Windows 11의 둥근 창 모서리, Mica, 일부 UWP / WinUI 앱(설정 앱 등)은 클래식 색상·글꼴을 따르지 않습니다.
- 창 테두리 비트맵 스킨, 비서명 msstyles, 시스템 파일 테마 패치는 v0.1 범위 밖입니다. **진짜 XP 창 테두리를 완전히 복제하지 않습니다.** 카드 이미지는 분위기를 설명하는 미리보기입니다.
- ExplorerPatcher는 일부 백신이 오탐할 수 있고, Windows 대형 업데이트 직후 새 호환 버전이 필요할 수 있습니다. 고정 버전이 현재 Windows 업데이트와 맞는지 테스트 VM에서 먼저 확인하세요.
- 고정 EP 설치 프로그램 자체가 탐색기를 종료하고 다시 띄우며, 같은 사용자로 상승 실행할 때 **잠시 상승된 탐색기가 존재할 가능성**이 있습니다. EasyRetro의 마지막 단계는 EP가 띄운 것을 포함해 **현재 세션의 모든 탐색기 프로세스**를 종료하고, 자동 재시작 또는 `/RL LIMITED /IT` 예약 작업으로 일반 권한 쉘을 시작한 뒤 토큰을 확인합니다. 다른 세션의 프로세스는 종료하지 않습니다. EP 자체의 재시작 동작을 없애는 옵션은 upstream에 없습니다. [고정 설치 소스](https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/ep_setup/ep_setup.c) · [upstream 쉘 시작 코드](https://github.com/valinet/ExplorerPatcher/blob/26100.8457.70.3/ExplorerPatcher/utility.h)
- 모니터별 원본 배경, 슬라이드쇼 목록·간격·셔플·사용 상태와 배경 캐시를 별도로 보관합니다. 복구 시 원본 모니터/슬라이드쇼 폴더에 접근할 수 없으면 백업을 보존하고 오류를 안내합니다. XP 배경은 출시본 Bliss 800×600이며 화면 채우기 확대는 Windows가 처리합니다. 현대 Windows가 비활성화한 시작/종료/로그인/로그오프 이벤트는 올바른 원본 WAV를 설정해도 재생되지 않을 수 있습니다. 이 프로그램은 재생을 강제하는 예약 작업이나 로그인/시스템 정책을 추가하지 않습니다.
- 이번 XP 원본 변경은 경고를 오류로 취급하는 C# 5 빌드, 모든 원본/미러 URL 8개·의존성 해시, WAV 22개, 무변경 계획·상태 출력, 이미지 실물 확인, 별도 임시 하네스의 픽셀 일치/캐시 손상/미러 재시도/항목별·전체 대체/에셋 롤백까지 검증했습니다. **실제 적용/제거 및 탐색기 재시작은 사용자의 PC 보호를 위해 실행하지 않았습니다.** Open-Shell에서의 최종 DPI·작업표시줄 표시와 테마 전환/원상복구 통합 동작은 VM에서 검증 후 사용하세요.

## 고급 사용 · 데이터 위치

```text
EasyRetro.exe --status
EasyRetro.exe --dry-run --apply 98
EasyRetro.exe --dry-run --apply xp
EasyRetro.exe --dry-run --restore
EasyRetro.exe --selftest
```

위 명령은 테마/레지스트리 설정을 바꾸거나 구성요소를 설치하지 않습니다. `--selftest`는 임시 폴더에 미리보기/98 에셋을 생성하고 **모든 XP 원본 URL과 미러를 각각 실제 다운로드**해 URL별 PASS/FAIL·SHA-256·크기를 출력합니다. 원본 이미지 변환, 모든 WAV의 RIFF/fmt/data 헤더(Windows 호환 PCM/ADPCM), 기존 두 의존성 해시와 OS 빌드도 검사합니다. 어느 URL이든 실패하거나 대체 에셋이 필요하면 실패 종료합니다. 검사 결과의 임시 폴더는 이미지 확인 후 직접 지워도 됩니다. `--dry-run --apply xp`는 각 원본/미러 URL·해시·대상 파일과 이벤트 매핑을 나열하고 다운로드하지 않습니다. PowerShell 5에서 한글 계획 출력을 파이프로 읽는 경우 먼저 `[Console]::OutputEncoding = [Text.UTF8Encoding]::new()`를 설정하세요.

실제 작업용 CLI는 `--apply 98|xp`, `--restore`, `--uninstall`이며 필요한 경우 자동으로 UAC 승격합니다. 중복 작업은 차단합니다.

- 매니저: `%LOCALAPPDATA%\Programs\EasyRetro\EasyRetro.exe`
- 상태·원본 백업: `%LOCALAPPDATA%\EasyRetro\state.json`, `backup\`
- 런타임 에셋: `assets\`, SHA-256 검증 다운로드 캐시/변환 준비 폴더: `cache\`, 로그: `logs\` (저장소 제외)
- 복구 성공 시 상태·원본/전환 백업·생성 에셋과 새로 설치한 구성요소의 사용자 캐시를 정리하고 로그와 다운로드 캐시는 남깁니다. 기존 구성요소는 제거하지 않으며, 새 설치 전부터 존재한 캐시는 원본대로 복원합니다.

레지스트리 값/설치 명령의 고정 버전 출처는 [`src/Core/Dependencies.cs`](src/Core/Dependencies.cs), XP 자산 URL·해시·추출/이벤트 매핑은 [`src/Core/XpAssets.cs`](src/Core/XpAssets.cs)에 있습니다. Open-Shell 스킨 활성화 값은 `CustomTaskbar`이며, Classic 스타일의 실제 스킨 이름은 `Classic Skin`입니다.

원본 스냅샷에는 두 테마가 사용하는 값뿐 아니라 SPI가 간접 변경하는 Desktop/WindowMetrics, 배경·테마 캐시, EP/Open-Shell 설정 트리와 설치 프로그램이 변경하는 정책/RunOnce, HKLM 자동실행·ShellServiceObject·COM 등록 값도 포함합니다. 최초 원본 스냅샷은 전환 중 덮어쓰지 않습니다. 백업 JSON은 임시 파일을 디스크에 플러시한 뒤 원자적으로 교체합니다. EP 제거 캐시 파일을 지웠다면 검증된 설치 파일을 다시 내려받기 위해 인터넷이 필요합니다.

## 소스에서 빌드

Windows에서 저장소 루트의 `build.cmd`를 실행하면 `dist\EasyRetro.exe`가 만들어집니다. 별도 SDK 없이 Windows 내장 .NET Framework 64비트 `csc.exe`를 사용합니다(C# 5). `v*` 태그를 푸시하면 GitHub Actions가 같은 스크립트로 빌드해 GitHub Release에 실행 파일을 첨부합니다.
